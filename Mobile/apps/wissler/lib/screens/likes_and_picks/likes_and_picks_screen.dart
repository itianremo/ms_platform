import 'dart:async';
import 'dart:ui';
import 'package:api_client/api_client.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import '../../widgets/wissler_header.dart';
import '../../widgets/custom_toast.dart';
import '../discover/user_details_screen.dart';
import '../profile/user_profile_screen.dart';

class LikesScreen extends ConsumerStatefulWidget {
  const LikesScreen({super.key});

  @override
  ConsumerState<LikesScreen> createState() => _LikesScreenState();
}

class _LikesScreenState extends ConsumerState<LikesScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final bool _isPremium = false;
  int _coins = 50; // Mocked Balance
  Timer? _timer;
  Duration _timeLeft = const Duration(days: 6, hours: 23, minutes: 59);

  // Toggle for My Likes (0: Likes, 1: Dislikes)
  final List<bool> _selection = [true, false];

  // Track unlocked users locally
  final Set<String> _unlockedUsers = {};

  // Internal Navigation State
  Recommendation? _selectedUser;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _startTimer();
  }

  void _openWisslerPlus() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      useRootNavigator: true,
      useSafeArea: true,
      backgroundColor: Colors.transparent,
      builder: (context) => const ClipRRect(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          child: UserProfileScreen(isModal: true)),
    );
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(minutes: 1), (timer) {
      if (mounted) {
        setState(() {
          _timeLeft = _timeLeft - const Duration(minutes: 1);
        });
      }
    });
  }

  int _calculatePrice(int index) {
    if (index == 0) return 0; // First is free
    if (index <= 5) return 5; // Next 5 cost 5
    // Each next 4 increases by 5
    // Indices 6-9 (4 items) -> 10
    // Indices 10-13 (4 items) -> 15
    int tier = (index - 6) ~/ 4;
    return 10 + (tier * 5);
  }

  void _handleUnlock(String userId, int price) {
    showDialog(
        context: context,
        builder: (context) => AlertDialog(
              backgroundColor: Colors.white,
              title: Text("Unlock Profile",
                  style: TextStyle(
                      color: Theme.of(context).primaryColor,
                      fontWeight: FontWeight.bold)),
              content: Column(mainAxisSize: MainAxisSize.min, children: [
                Text("This action costs $price coins."),
                const SizedBox(height: 10),
                Row(children: [
                  const Text("Your Balance: "),
                  const Icon(Icons.monetization_on,
                      color: Colors.amber, size: 16),
                  Text(" $_coins",
                      style: const TextStyle(fontWeight: FontWeight.bold))
                ]),
                const SizedBox(height: 20),
                if (_coins < price)
                  const Text("Insufficient funds!",
                      style: TextStyle(
                          color: Colors.red, fontWeight: FontWeight.bold))
              ]),
              actions: [
                TextButton(
                    onPressed: () => Navigator.pop(context),
                    child: const Text("Cancel")),
                if (_coins >= price)
                  ElevatedButton(
                      style: ElevatedButton.styleFrom(
                          backgroundColor: Theme.of(context).primaryColor),
                      onPressed: () {
                        setState(() {
                          _coins -= price;
                          _unlockedUsers.add(userId); // Persist unlock
                        });
                        Navigator.pop(context);
                        CustomToast.show(context, "Profile Unlocked!");
                      },
                      child: const Text("Unlock",
                          style: TextStyle(color: Colors.white)))
                else
                  ElevatedButton(
                      style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.amber),
                      onPressed: () {
                        Navigator.pop(context);
                        _openWisslerPlus();
                      },
                      child: const Text("Top Up",
                          style: TextStyle(color: Colors.white)))
              ],
            ));
  }

  @override
  void dispose() {
    _tabController.dispose();
    _timer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_selectedUser != null) {
      return UserDetailsScreen(
        profile: _selectedUser,
        onBack: () {
          setState(() {
            _selectedUser = null;
          });
        },
      );
    }

    return Scaffold(
      appBar: const WisslerHeader(title: "Likes & Picks"),
      body: Column(children: [
        TabBar(
          controller: _tabController,
          labelColor: Theme.of(context).primaryColor,
          unselectedLabelColor: Colors.grey,
          indicatorColor: Colors.amber,
          tabs: const [
            Tab(text: "Who Likes Me"),
            Tab(text: "My Likes"),
            Tab(text: "Top Picks"),
          ],
        ),
        Expanded(
            child: TabBarView(
          controller: _tabController,
          children: [
            _buildWhoLikesMe(),
            _buildMyLikes(),
            _buildTopPicks(),
          ],
        ))
      ]),
    );
  }

  Widget _buildResetTimer() {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 8),
      color: Colors.white,
      width: double.infinity,
      alignment: Alignment.center,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.timer, size: 16, color: Theme.of(context).primaryColor),
          const SizedBox(width: 5),
          Text(
            "Resets in ${_timeLeft.inDays}d ${_timeLeft.inHours % 24}h ${_timeLeft.inMinutes % 60}m",
            style: TextStyle(
                color: Theme.of(context).primaryColor,
                fontWeight: FontWeight.bold,
                fontSize: 12),
          ),
        ],
      ),
    );
  }

  Widget _buildWhoLikesMe() {
    return Column(
      children: [
        _buildResetTimer(),
        Expanded(
          child: Stack(
            children: [
              Positioned.fill(
                child: _PagedProfileList(
                  fetcher: (client, uid) => client.getWhoLikesMe(uid),
                  itemBuilder: (context, user) => const SizedBox(),
                  itemBuilderWithIndex: (context, user, index) {
                    // Only 1st is free/revealed by default OR if unlocked locally
                    bool isRevealed = _isPremium ||
                        index == 0 ||
                        _unlockedUsers.contains(user.userId);
                    int price = _calculatePrice(index);

                    return GestureDetector(
                      onTap: () {
                        if (isRevealed) {
                          setState(() {
                            _selectedUser = user;
                          });
                        } else {
                          // Optional: Prompt unlock on card tap too
                          _handleUnlock(user.userId, price);
                        }
                      },
                      child: _ProfileCard(
                        user: user,
                        isRevealed: isRevealed,
                        unlockPrice: isRevealed ? 0 : price,
                        onUnlock: () => _handleUnlock(user.userId, price),
                      ),
                    );
                  },
                  isGrid: true,
                ),
              ),
              if (!_isPremium)
                Positioned(
                  bottom: 20,
                  left: 20,
                  right: 20,
                  child: SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                          onPressed: () {
                            _openWisslerPlus();
                          },
                          style: ElevatedButton.styleFrom(
                              backgroundColor: Theme.of(context).primaryColor,
                              padding:
                                  const EdgeInsets.symmetric(vertical: 15)),
                          child: const Text("See Who Likes You",
                              style: TextStyle(
                                  color: Colors.white, // White for contrast
                                  fontWeight: FontWeight.bold)))),
                )
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildMyLikes() {
    return Column(
      children: [
        // Custom Toggle Buttons (Redesigned)
        Padding(
          padding: const EdgeInsets.all(16.0),
          child: Container(
            decoration: BoxDecoration(
              color: Colors.grey[200],
              borderRadius: BorderRadius.circular(30),
            ),
            padding: const EdgeInsets.all(4),
            child: Row(
              children: [
                _buildToggleBtn("Likes", 0),
                _buildToggleBtn("Dislikes", 1),
              ],
            ),
          ),
        ),
        Expanded(
          child: _PagedProfileList(
            fetcher: (client, uid) async {
              // Fetch likes or dislikes based on selection
              final results = _selection[0]
                  ? await client.getMyLikes(uid)
                  : await client.getMyDislikes(uid);

              // Limit to recent 20
              if (results.length > 20) {
                return results.sublist(results.length - 20).reversed.toList();
              }
              return results.reversed.toList();
            },
            itemBuilder: (context, user) => const SizedBox(),
            itemBuilderWithIndex: (context, user, index) {
              // 1st is free/unlocked. Rest are locked unless locally unlocked.
              bool isUnlocked =
                  index == 0 || _unlockedUsers.contains(user.userId);
              int price = 5 + (index * 2);

              return SizedBox(
                height: 140, // Constrain height for List View
                child: GestureDetector(
                  onTap: () {
                    if (isUnlocked) {
                      setState(() {
                        _selectedUser = user;
                      });
                    } else {
                      _handleUnlock(user.userId, price);
                    }
                  },
                  child: _ProfileCard(
                    user: user,
                    isRevealed: isUnlocked,
                    unlockPrice: isUnlocked ? 0 : price,
                    isLocked: !isUnlocked,
                    onUnlock: () => _handleUnlock(user.userId, price),
                  ),
                ),
              );
            },
            key: ValueKey(_selection[0]),
            isGrid: false,
          ),
        ),
      ],
    );
  }

  Widget _buildToggleBtn(String label, int index) {
    final isSelected = _selection[index];
    return Expanded(
      child: GestureDetector(
        onTap: () {
          setState(() {
            for (int i = 0; i < _selection.length; i++) {
              _selection[i] = i == index;
            }
          });
        },
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(vertical: 12),
          decoration: BoxDecoration(
            color: isSelected ? Colors.white : Colors.transparent,
            borderRadius: BorderRadius.circular(25),
            boxShadow: isSelected
                ? [
                    BoxShadow(
                        color: Colors.black.withOpacity(0.1),
                        blurRadius: 5,
                        offset: const Offset(0, 2))
                  ]
                : [],
          ),
          alignment: Alignment.center,
          child: Text(
            label,
            style: TextStyle(
              fontWeight: FontWeight.bold,
              color: isSelected
                  ? Theme.of(context).primaryColor
                  : Colors.grey[600],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildTopPicks() {
    return Column(
      children: [
        _buildResetTimer(), // Mirroring Who Likes Me (User request: "do the same")
        Expanded(
          child: Stack(
            children: [
              Positioned.fill(
                child: _PagedProfileList(
                  fetcher: (client, uid) => client.getTopPicks(uid),
                  itemBuilder: (context, user) => const SizedBox(),
                  itemBuilderWithIndex: (context, user, index) {
                    // Only 1st is free/revealed by default (User request: "only show one card")
                    // If they mean "List with only 1 unlocked", this works.
                    // If they mean "List of 1 item", that's different.
                    // "do the same we did on other cards including who likes me" implies the blurring list mechanism.
                    bool isLocked = !_isPremium &&
                        index > 0 && // Only index 0 is free
                        !_unlockedUsers.contains(user.userId);
                    int price = _calculatePrice(index);

                    return GestureDetector(
                        onTap: () {
                          if (isLocked) {
                            _handleUnlock(user.userId, price);
                            return;
                          }
                          setState(() {
                            _selectedUser = user;
                          });
                        },
                        child: _ProfileCard(
                          user: user,
                          isRevealed:
                              true, // Top Picks are "revealed" content-wise but covered by lock
                          isLocked: isLocked, // Handles blur and lock icon
                          onUnlock: () => _handleUnlock(user.userId, price),
                          unlockPrice: price,
                          showMatchPercentage: true, // Enable Match Badge
                          matchPercentage:
                              98 - (index * 2), // Mock Match %: 98, 96, 94...
                        ));
                  },
                  isGrid: true,
                ),
              ),
              if (!_isPremium)
                Positioned(
                  bottom: 20,
                  left: 20,
                  right: 20,
                  child: SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                          onPressed: () {
                            _openWisslerPlus();
                          },
                          style: ElevatedButton.styleFrom(
                              backgroundColor: Theme.of(context).primaryColor,
                              padding:
                                  const EdgeInsets.symmetric(vertical: 15)),
                          child: const Text("Unlock Top Picks",
                              style: TextStyle(
                                  color: Colors.white, // White for contrast
                                  fontWeight: FontWeight.bold)))),
                )
            ],
          ),
        ),
      ],
    );
  }
}

class _ProfileCard extends StatelessWidget {
  final Recommendation user;
  final bool isRevealed;
  final bool isLocked;
  final VoidCallback? onUnlock;
  final int unlockPrice;
  final bool showMatchPercentage;
  final int matchPercentage;

  const _ProfileCard({
    required this.user,
    this.isRevealed = true,
    this.isLocked = false,
    this.onUnlock,
    this.unlockPrice = 0,
    this.showMatchPercentage = false,
    this.matchPercentage = 0,
  });

  @override
  Widget build(BuildContext context) {
    // Wissler Admin Light Theme Colors
    const adminBlue = Color(0xFF003366);
    const orange = Colors.amber; // "the orange color already used"

    return Card(
      clipBehavior: Clip.antiAlias,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
      child: Stack(fit: StackFit.expand, children: [
        // 1. Image (Blurred if locked/hidden)
        ImageFiltered(
            imageFilter: ImageFilter.blur(
                sigmaX: (!isRevealed || isLocked) ? 10 : 0,
                sigmaY: (!isRevealed || isLocked) ? 10 : 0),
            child: Image.network(user.avatarUrl,
                fit: BoxFit.cover, width: double.infinity)),

        // 2. Gradient Overlay (Blue-ish tint if desired, or standard black for contrast)
        // User asked for "same color of wissler admin light theme", so let's use Admin Blue tint
        if (isRevealed && !isLocked)
          Container(
              decoration: BoxDecoration(
                  gradient: LinearGradient(
                      colors: [Colors.transparent, adminBlue.withOpacity(0.9)],
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter))),

        // 3. Badges (Top Left & Bottom Left) - Orange Themed
        if (isRevealed && !isLocked) ...[
          // Match Percentage Badge (Top Right) - Requested Feature
          if (showMatchPercentage)
            Positioned(
              top: 8,
              right: 8,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.orange,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.white, width: 1),
                  boxShadow: [
                    BoxShadow(
                        color: Colors.black.withOpacity(0.2),
                        blurRadius: 4,
                        offset: const Offset(0, 2))
                  ],
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.local_fire_department,
                        color: Colors.white, size: 16),
                    const SizedBox(width: 4),
                    Text(
                      "$matchPercentage%",
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
            ),

          // Active Badge
          Positioned(
              top: 8,
              left: 8,
              child: Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                      color: orange, // Orange Background (was Blue)
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.white.withOpacity(0.5))),
                  child: const Row(mainAxisSize: MainAxisSize.min, children: [
                    Icon(Icons.access_time,
                        color: adminBlue, size: 10), // Blue Icon
                    SizedBox(width: 4),
                    Text("Active 2h ago",
                        style: TextStyle(
                            color: adminBlue, // Blue Text
                            fontSize: 10,
                            fontWeight: FontWeight.bold))
                  ]))),

          // Details (Bottom)
          Positioned(
              bottom: 10,
              left: 10,
              right: 10,
              child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Gender & Age Badge
                    Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 6, vertical: 2),
                        margin: const EdgeInsets.only(bottom: 5),
                        decoration: BoxDecoration(
                            color: orange, // Orange Background
                            borderRadius: BorderRadius.circular(4)),
                        child: Row(mainAxisSize: MainAxisSize.min, children: [
                          const Icon(Icons.female,
                              color: Color(0xFF003366), size: 12),
                          const SizedBox(width: 4),
                          Text("${user.age}",
                              style: const TextStyle(
                                  color: Color(0xFF003366),
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold))
                        ])),

                    // Name
                    Text(user.displayName,
                        style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 16)),
                    // Location
                    Row(
                      children: [
                        const Icon(Icons.location_on,
                            color: Colors.white70, size: 12),
                        const SizedBox(width: 2),
                        Expanded(
                          child: Text("${user.city}, ${user.country}",
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                  color: Colors.white70, fontSize: 12)),
                        ),
                      ],
                    )
                  ]))
        ],

        // 4. Locked / Hidden State
        if (!isRevealed || isLocked) ...[
          // Center Content
          Center(
              child: Column(mainAxisSize: MainAxisSize.min, children: [
            if (isLocked || (!isRevealed && unlockPrice > 0)) ...[
              const Icon(Icons.lock, color: Colors.white, size: 30),
              const SizedBox(height: 5),
              Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.monetization_on,
                      color: Colors.amber, size: 16),
                  const SizedBox(width: 4),
                  Text("$unlockPrice",
                      style: const TextStyle(
                          color: Colors.amber,
                          fontWeight: FontWeight.bold,
                          fontSize: 18)),
                ],
              ),
              const SizedBox(height: 10),
              ElevatedButton(
                  onPressed: onUnlock,
                  style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.amber,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 15, vertical: 0),
                      minimumSize: const Size(0, 30)),
                  child: const Text("Unlock",
                      style: TextStyle(fontWeight: FontWeight.bold)))
            ] else if (isLocked)
              // Only Locked (Top Picks case where price might not apply same way or is pure premium)
              ElevatedButton(
                  onPressed: onUnlock,
                  style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.amber,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 15, vertical: 0),
                      minimumSize: const Size(0, 30)),
                  child: const Text("Unlock",
                      style: TextStyle(fontWeight: FontWeight.bold)))
            else
              const Text("Start matching",
                  style: TextStyle(
                      color: Colors.white, fontWeight: FontWeight.bold))
          ]))
        ]
      ]),
    );
  }
}

class _PagedProfileList extends ConsumerStatefulWidget {
  final Future<List<Recommendation>> Function(AuthClient, String) fetcher;
  final Widget Function(BuildContext, Recommendation) itemBuilder;
  final Widget Function(BuildContext, Recommendation, int)?
      itemBuilderWithIndex; // Added support for index
  final bool isGrid;

  const _PagedProfileList(
      {super.key,
      required this.fetcher,
      required this.itemBuilder,
      required this.isGrid,
      this.itemBuilderWithIndex});

  @override
  ConsumerState<_PagedProfileList> createState() => _PagedProfileListState();
}

class _PagedProfileListState extends ConsumerState<_PagedProfileList> {
  List<Recommendation> _items = [];
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _fetchData();
  }

  Future<void> _fetchData() async {
    if (_isLoading) return;

    // Initial Load
    if (_items.isEmpty) {
      setState(() => _isLoading = true);
    }

    try {
      final client = ref.read(authClientProvider);
      // Timeout handling
      const userId = "22222222-2222-2222-2222-222222222222";
      final newItems = await widget
          .fetcher(client, userId)
          .timeout(const Duration(seconds: 10));

      if (mounted) {
        setState(() {
          _items = newItems; // Replace items on refresh/load
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        CustomToast.show(context,
            "Wissler is whistling to the server, you may refresh later");
      }
    }
  }

  Future<void> _onRefresh() async {
    await _fetchData();
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading && _items.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: _onRefresh,
      child: _items.isEmpty
          ? SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              child: SizedBox(
                  height: MediaQuery.of(context).size.height * 0.5,
                  child: const Center(child: Text("No profiles found."))))
          : widget.isGrid
              ? GridView.builder(
                  padding: const EdgeInsets.all(8),
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      childAspectRatio: 0.75,
                      crossAxisSpacing: 10,
                      mainAxisSpacing: 10),
                  itemCount: _items.length,
                  itemBuilder: (context, index) =>
                      widget.itemBuilderWithIndex != null
                          ? widget.itemBuilderWithIndex!(
                              context, _items[index], index)
                          : widget.itemBuilder(context, _items[index]),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(8),
                  itemCount: _items.length,
                  itemBuilder: (context, index) =>
                      widget.itemBuilderWithIndex != null
                          ? widget.itemBuilderWithIndex!(
                              context, _items[index], index)
                          : widget.itemBuilder(context, _items[index]),
                ),
    );
  }
}
