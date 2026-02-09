import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import '../../theme/dynamic_theme_provider.dart';
import '../../theme/dynamic_theme_provider.dart';
import '../../widgets/custom_toast.dart';
import '../home/wissler_home_screen.dart';

class UserProfileScreen extends ConsumerStatefulWidget {
// ... (omitted)

  final bool isModal;
  final VoidCallback? onClose;
  const UserProfileScreen({super.key, this.isModal = false, this.onClose});

  @override
  ConsumerState<UserProfileScreen> createState() => _UserProfileScreenState();
}

class _UserProfileScreenState extends ConsumerState<UserProfileScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  UserProfile? _userProfile;
  bool _isLoading = true;

  // Lookups
  List<Country> _countries = [];
  List<City> _cities = [];

  // Gamification
  int _coins = 0;
  int _score = 0;

  // Packages
  List<SubscriptionPackage> _packages = [];
  bool _isLoadingPackages = false;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadProfile();
    _loadLookups();
    _loadPackages();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadPackages() async {
    setState(() => _isLoadingPackages = true);
    try {
      final client = ref.read(authClientProvider);
      final pkgs = await client
          .getSubscriptionPackages('22222222-2222-2222-2222-222222222222');
      if (mounted)
        setState(() => _packages = pkgs.where((p) => p.isActive).toList());
    } catch (_) {
    } finally {
      if (mounted) setState(() => _isLoadingPackages = false);
    }
  }

  Future<void> _loadLookups() async {
    try {
      final client = ref.read(authClientProvider);
      final countries = await client.getCountries();
      if (mounted) setState(() => _countries = countries);
    } catch (_) {}
  }

  Future<void> _loadProfile() async {
    setState(() => _isLoading = true);
    try {
      final repo = ref.read(authRepositoryProvider);
      final token = repo.getAccessToken();
      if (token == null) throw Exception('User not logged in');
      final payload = _decodeJwt(token);
      final userId = payload['sub'] ?? payload['id'] ?? payload['userId'];

      final client = ref.read(authClientProvider);
      final profile = await client.getUserProfile(
          userId, '22222222-2222-2222-2222-222222222222');

      if (profile != null) {
        setState(() {
          _userProfile = profile;
          _parseCustomData(profile.customDataJson);
        });
      }
    } catch (_) {
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _parseCustomData(String? jsonStr) {
    if (jsonStr == null) return;
    try {
      final data = json.decode(jsonStr);
      if (data is Map<String, dynamic>) {
        setState(() {
          _coins = data['coins'] ?? 0;
          _score = data['score'] ?? 0;
        });
      }
    } catch (_) {}
  }

  void _logout() => ref.read(authProvider.notifier).logout();

  Map<String, dynamic> _decodeJwt(String token) {
    final parts = token.split('.');
    if (parts.length != 3) return {};
    final payload = parts[1];
    final normalized = base64Url.normalize(payload);
    final resp = utf8.decode(base64Url.decode(normalized));
    return json.decode(resp);
  }

  String _calculateAge(DateTime? dob) {
    if (dob == null) return "N/A";
    final now = DateTime.now();
    int age = now.year - dob.year;
    if (now.month < dob.month || (now.month == dob.month && now.day < dob.day))
      age--;
    return age.toString();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return Scaffold(
      body: NestedScrollView(
        headerSliverBuilder: (context, _) => [
          SliverAppBar(
            expandedHeight: 180, // Reduced height for narrower spacing
            leading: widget.isModal
                ? IconButton(
                    icon: Icon(Icons.close, color: Colors.grey[800], size: 30),
                    onPressed: () {
                      if (widget.onClose != null) {
                        widget.onClose!();
                      } else {
                        Navigator.pop(context);
                      }
                    })
                : null,
            automaticallyImplyLeading: false,
            pinned: true,
            flexibleSpace: FlexibleSpaceBar(
              background: Container(
                decoration: BoxDecoration(
                    gradient: LinearGradient(colors: [
                  theme.primaryColor,
                  isDark ? Colors.black : Colors.white
                ], begin: Alignment.topCenter, end: Alignment.bottomCenter)),
                padding: EdgeInsets.only(
                    top: MediaQuery.of(context).padding.top +
                        (widget.isModal ? 40 : 20),
                    left: 20,
                    right: 20),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Avatar & Info Column
                    Expanded(
                      flex: 3,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              CircleAvatar(
                                radius: 35,
                                backgroundImage: _userProfile?.avatarUrl != null
                                    ? NetworkImage(_userProfile!.avatarUrl!)
                                    : null,
                                child: _userProfile?.avatarUrl == null
                                    ? const Icon(Icons.person, size: 35)
                                    : null,
                              ),
                              const SizedBox(width: 15),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      "${_userProfile?.displayName ?? 'Guest'}, ${_calculateAge(_userProfile?.dateOfBirth)}",
                                      style: const TextStyle(
                                          color: Colors.white,
                                          fontWeight: FontWeight.bold,
                                          fontSize: 18),
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    const Text("Cairo, Egypt",
                                        style: TextStyle(
                                            color: Colors.white70,
                                            fontSize: 14)),
                                  ],
                                ),
                              )
                            ],
                          ),
                          const SizedBox(height: 10),
                          Row(
                            children: [
                              _buildHeaderBtn(context, Icons.edit, () {
                                if (_userProfile != null) {
                                  // Open Edit in Home Screen Overlay
                                  final homeState = wisslerHomeKey.currentState;
                                  if (homeState != null) {
                                    homeState.openEditProfile(
                                        _userProfile!,
                                        _countries,
                                        _cities); // Note: Need _cities in State or pass empty
                                  }
                                }
                              }),
                              const SizedBox(width: 15),
                              _buildHeaderBtn(context, Icons.visibility, () {
                                if (_userProfile != null) {
                                  // Open Preview in Home Screen Overlay
                                  final homeState = wisslerHomeKey.currentState;
                                  if (homeState != null) {
                                    homeState.openPreviewProfile(_userProfile!);
                                  }
                                }
                              }),
                            ],
                          )
                        ],
                      ),
                    ),
                    // Gamification Column (Thinner)
                    Expanded(
                      flex: 2,
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: [
                          _buildThinCard(
                              "Loyalty", "$_score", Icons.star, Colors.indigo),
                          const SizedBox(height: 5),
                          _buildThinCard("Coins", "$_coins",
                              Icons.monetization_on, Colors.amber),
                        ],
                      ),
                    )
                  ],
                ),
              ),
            ),
          ),
          SliverPersistentHeader(
            delegate: _SliverTabBarDelegate(
              TabBar(
                controller: _tabController,
                indicatorColor: Colors.orange,
                labelColor: Colors.orange,
                unselectedLabelColor: Colors.blueGrey,
                labelStyle: const TextStyle(
                    fontWeight: FontWeight.normal, fontSize: 12),
                tabs: const [
                  Tab(
                      child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                        Icon(Icons.stars, size: 16),
                        SizedBox(width: 5),
                        Text("Wissler+")
                      ])),
                  Tab(
                      child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                        Icon(Icons.notifications, size: 16),
                        SizedBox(width: 5),
                        Text("Alerts")
                      ])),
                  Tab(
                      child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                        Icon(Icons.settings, size: 16),
                        SizedBox(width: 5),
                        Text("Settings")
                      ])),
                ],
              ),
            ),
            pinned: true,
          ),
        ],
        body: TabBarView(
          controller: _tabController,
          children: [
            _buildWisslerPlusTab(),
            _buildNotificationsTab(),
            _buildSettingsTab()
          ],
        ),
      ),
    );
  }

  Widget _buildHeaderBtn(
      BuildContext context, IconData icon, VoidCallback onTap) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(right: 15),
        padding: const EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: Colors.white.withOpacity(0.15),
          shape: BoxShape.circle,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 8,
              spreadRadius: 1,
            )
          ],
        ),
        child: Icon(icon, color: Colors.white, size: 22),
      ),
    );
  }

  Widget _buildThinCard(
      String title, String value, IconData icon, Color color) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 10),
      decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(10),
          boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 4)]),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: color, size: 20),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(value,
                  style: const TextStyle(
                      fontSize: 16, fontWeight: FontWeight.bold)),
              Text(title,
                  style: const TextStyle(fontSize: 10, color: Colors.grey)),
            ],
          )
        ],
      ),
    );
  }

  Widget _buildWisslerPlusTab() {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        // Current Subscription Status (Mock)
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            gradient: const LinearGradient(
                colors: [Colors.purple, Colors.deepPurple]),
            borderRadius: BorderRadius.circular(15),
          ),
          child: const Row(
            children: [
              Icon(Icons.diamond, color: Colors.white, size: 40),
              SizedBox(width: 15),
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text("Free Plan",
                    style: TextStyle(
                        color: Colors.white,
                        fontSize: 18,
                        fontWeight: FontWeight.bold)),
                Text("Upgrade to unlock more!",
                    style: TextStyle(color: Colors.white70)),
              ])
            ],
          ),
        ),
        const SizedBox(height: 20),
        const Text("Premium Packages",
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
        const SizedBox(height: 10),
        SizedBox(
          height: 220,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            itemCount: _packages.length + 1, // +1 for mock if empty
            itemBuilder: (context, index) {
              if (_packages.isEmpty && index == 0) {
                return _buildPackageCard(
                    "Wissler VIP", "Get everything unlimited!", 19.99);
              }
              if (index >= _packages.length) return const SizedBox.shrink();
              final p = _packages[index];
              return _buildPackageCard(p.name, p.description, p.price);
            },
          ),
        ),

        const SizedBox(height: 20),
        const Text("Get Coins",
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
        const SizedBox(height: 10),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            _buildCoinOption("100", "\$1.99"),
            _buildCoinOption("500", "\$6.99", isBestValue: true),
            _buildCoinOption("1000", "\$11.99"),
          ],
        ),
        const SizedBox(height: 10),
        const Text("Use coins to buy Super Likes, Boosts, and more!",
            style: TextStyle(color: Colors.grey, fontSize: 12)),
      ],
    );
  }

  Widget _buildPackageCard(String name, String desc, double price) {
    return Container(
      width: 250,
      margin: const EdgeInsets.only(right: 15),
      padding: const EdgeInsets.all(15),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: Colors.purple.withOpacity(0.3)),
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 10)],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(name,
              style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: Colors.deepPurple)),
          const SizedBox(height: 5),
          Text(desc,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(color: Colors.grey)),
          const Spacer(),
          Text("\$${price.toStringAsFixed(2)} / mo",
              style:
                  const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          const SizedBox(height: 10),
          SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                  onPressed: () {
                    CustomToast.show(context, "Subscribing to $name...");
                  },
                  style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.purple,
                      foregroundColor: Colors.white),
                  child: const Text("Select")))
        ],
      ),
    );
  }

  Widget _buildCoinOption(String coins, String price,
      {bool isBestValue = false}) {
    return Expanded(
      child: GestureDetector(
        onTap: () {
          showDialog(
              context: context,
              builder: (_) => AlertDialog(
                      title: const Text("Confirm Purchase"),
                      content: Text("Buy $coins Coins for $price?"),
                      actions: [
                        TextButton(
                            onPressed: () => Navigator.pop(context),
                            child: const Text("Cancel")),
                        ElevatedButton(
                            onPressed: () {
                              Navigator.pop(context);
                              CustomToast.show(
                                  context, "Purchased $coins Coins!");
                            },
                            child: const Text("Buy")),
                      ]));
        },
        child: Container(
          margin: const EdgeInsets.symmetric(horizontal: 5),
          padding: const EdgeInsets.symmetric(vertical: 15),
          decoration: BoxDecoration(
            color: isBestValue ? Colors.amber[100] : Colors.grey[100],
            border: isBestValue ? Border.all(color: Colors.amber) : null,
            borderRadius: BorderRadius.circular(15),
          ),
          child: Column(
            children: [
              if (isBestValue)
                const Text("BEST VALUE",
                    style: TextStyle(
                        fontSize: 10,
                        fontWeight: FontWeight.bold,
                        color: Colors.orange)),
              Icon(Icons.monetization_on,
                  color: isBestValue ? Colors.amber : Colors.grey, size: 30),
              const SizedBox(height: 5),
              Text(coins,
                  style: const TextStyle(
                      fontWeight: FontWeight.bold, fontSize: 18)),
              Text(price, style: const TextStyle(color: Colors.grey)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildNotificationsTab() {
    return ListView.separated(
        padding: const EdgeInsets.all(10),
        itemCount: 5,
        separatorBuilder: (_, __) => const Divider(),
        itemBuilder: (context, index) => ListTile(
              leading: const CircleAvatar(
                  backgroundColor: Colors.pink,
                  child: Icon(Icons.favorite, color: Colors.white, size: 16)),
              title: Text("New Like! #$index"),
              subtitle: const Text("Someone liked your profile."),
              trailing: const Text("2m ago",
                  style: TextStyle(fontSize: 12, color: Colors.grey)),
            ));
  }

  Widget _buildSettingsTab() {
    return ListView(padding: const EdgeInsets.all(20), children: [
      SwitchListTile(
          title: const Text("Dark Mode"),
          value: ref.watch(dynamicThemeProvider).brightness == Brightness.dark,
          onChanged: (v) =>
              ref.read(dynamicThemeProvider.notifier).toggleDarkMode(v)),
      ListTile(
          title: const Text("Privacy Policy"),
          trailing: const Icon(Icons.arrow_forward_ios),
          onTap: () {}),
      ListTile(
          title: const Text("Log Out"),
          trailing: const Icon(Icons.logout, color: Colors.red),
          onTap: _logout),
    ]);
  }
}

class _SliverTabBarDelegate extends SliverPersistentHeaderDelegate {
  final TabBar _tabBar;

  _SliverTabBarDelegate(this._tabBar);

  @override
  double get minExtent => _tabBar.preferredSize.height;
  @override
  double get maxExtent => _tabBar.preferredSize.height;

  @override
  Widget build(
      BuildContext context, double shrinkOffset, bool overlapsContent) {
    return Container(
      color: Theme.of(context).scaffoldBackgroundColor,
      child: _tabBar,
    );
  }

  @override
  bool shouldRebuild(_SliverTabBarDelegate oldDelegate) {
    return false;
  }
}
