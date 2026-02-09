import 'package:api_client/api_client.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'dart:convert';

class UserDetailsScreen extends ConsumerStatefulWidget {
  final Recommendation?
      profile; // Make nullable to support direct UserProfile passing
  final UserProfile?
      userProfile; // Support passing full profile directly (e.g. Preview)
  final bool isMatched;
  final VoidCallback? onBack;

  const UserDetailsScreen(
      {super.key,
      this.profile,
      this.userProfile,
      this.isMatched = false,
      this.onBack})
      : assert(profile != null || userProfile != null);

  @override
  ConsumerState<UserDetailsScreen> createState() => _UserDetailsScreenState();
}

class _UserDetailsScreenState extends ConsumerState<UserDetailsScreen> {
  UserProfile? _fullProfile;
  bool _isLoading = true;

  // Custom Data Fields
  List<String> _photos = [];
  Map<String, dynamic> _attributes = {};

  // Mock photos for "Many Pix" experience if none exist
  final List<String> _mockPhotos = [
    "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=500&auto=format&fit=crop&q=60",
    "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=500&auto=format&fit=crop&q=60",
    "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=500&auto=format&fit=crop&q=60",
    "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=500&auto=format&fit=crop&q=60",
    "https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?w=500&auto=format&fit=crop&q=60"
  ];

  @override
  void initState() {
    super.initState();
    if (widget.userProfile != null) {
      _fullProfile = widget.userProfile;
      _parseData();
      _isLoading = false;
    } else {
      _loadProfile();
    }
  }

  void _parseData() {
    if (_fullProfile?.customDataJson == null) return;
    try {
      final data = json.decode(_fullProfile!.customDataJson!);
      _attributes = data;
      if (data['photos'] != null && (data['photos'] as List).isNotEmpty) {
        _photos = List<String>.from(data['photos']);
      }
    } catch (_) {}
  }

  Future<void> _loadProfile() async {
    try {
      final client = ref.read(authClientProvider);
      final uid = widget.profile!.userId;
      final full = await client.getUserProfile(
          uid, '22222222-2222-2222-2222-222222222222');
      if (mounted && full != null) {
        setState(() {
          _fullProfile = full;
          _parseData();
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final primary = Theme.of(context).primaryColor;

    // Resolve display data
    final displayName =
        _fullProfile?.displayName ?? widget.profile?.displayName ?? 'User';
    final age = _fullProfile?.dateOfBirth != null
        ? _calculateAge(_fullProfile!.dateOfBirth!)
        : (widget.profile?.age.toString() ?? 'N/A');
    final city =
        _attributes['cityId'] ?? widget.profile?.city ?? 'Unknown City';
    final country = _attributes['countryId'] ??
        widget.profile?.country ??
        'Unknown Country';
    final bio = _fullProfile?.bio ?? "No bio available.";

    // Photos logic: Use fetched photos, or fallback to mock photos for "Many Pix" experience
    List<String> displayPhotos = [];
    if (_photos.isNotEmpty) {
      displayPhotos = _photos;
    } else if (widget.profile?.avatarUrl != null) {
      displayPhotos = [
        widget.profile!.avatarUrl,
        ..._mockPhotos
      ]; // Mix real + mock
    } else if (_fullProfile?.avatarUrl != null) {
      displayPhotos = [_fullProfile!.avatarUrl!, ..._mockPhotos];
    } else {
      displayPhotos = _mockPhotos;
    }

    return Scaffold(
      body: Stack(
        children: [
          CustomScrollView(
            slivers: [
              SliverAppBar(
                expandedHeight:
                    MediaQuery.of(context).size.height * 0.55, // 55% height
                pinned: true,
                stretch: true,
                backgroundColor: primary,
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back,
                      color: Colors.white,
                      shadows: [Shadow(color: Colors.black, blurRadius: 10)]),
                  onPressed: () {
                    if (widget.onBack != null) {
                      widget.onBack!();
                    } else {
                      Navigator.pop(context);
                    }
                  },
                ),
                flexibleSpace: FlexibleSpaceBar(
                  background: PageView(
                    children: displayPhotos
                        .map((url) => Image.network(url, fit: BoxFit.cover))
                        .toList(),
                  ),
                ),
              ),
              SliverToBoxAdapter(
                child: Transform.translate(
                  offset: const Offset(0, -30),
                  child: Container(
                    decoration: const BoxDecoration(
                      color: Colors.white,
                      borderRadius:
                          BorderRadius.vertical(top: Radius.circular(40)),
                      boxShadow: [
                        BoxShadow(
                            color: Colors.black12,
                            blurRadius: 10,
                            offset: Offset(0, -5))
                      ],
                    ),
                    padding: const EdgeInsets.fromLTRB(24, 40, 24, 120),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Name & Age Row
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              "$displayName, $age",
                              style: const TextStyle(
                                fontSize: 28,
                                fontWeight: FontWeight.bold,
                                color: Colors.black87,
                              ),
                            ),
                            if (widget.profile?.isVerified == true ||
                                (_fullProfile != null &&
                                    _fullProfile?.customDataJson
                                            ?.contains('verified') ==
                                        true))
                              const Icon(Icons.verified,
                                  color: Colors.blue, size: 28)
                          ],
                        ),
                        // Location
                        const SizedBox(height: 5),
                        Row(
                          children: [
                            Icon(Icons.location_on,
                                size: 16, color: Colors.grey[600]),
                            const SizedBox(width: 4),
                            Text(
                              "$city, $country",
                              style: TextStyle(
                                  color: Colors.grey[700], fontSize: 16),
                            ),
                          ],
                        ),

                        const Divider(height: 40),

                        // About
                        const Text("About",
                            style: TextStyle(
                                fontSize: 18, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 10),
                        Text(bio,
                            style: const TextStyle(
                                fontSize: 16,
                                height: 1.5,
                                color: Colors.black54)),

                        const Divider(height: 40),

                        // Details Grid
                        const Text("Details",
                            style: TextStyle(
                                fontSize: 18, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 15),
                        Wrap(
                          spacing: 10,
                          runSpacing: 10,
                          children: [
                            if (_attributes['height'] != null)
                              _buildAttributeChip(
                                  Icons.height, "${_attributes['height']} cm"),
                            if (_attributes['weight'] != null)
                              _buildAttributeChip(Icons.monitor_weight,
                                  "${_attributes['weight']} kg"),
                            if (_attributes['eyeColor'] != null)
                              _buildAttributeChip(Icons.remove_red_eye,
                                  _attributes['eyeColor']),
                            if (_attributes['hairColor'] != null)
                              _buildAttributeChip(
                                  Icons.face, _attributes['hairColor']),
                            if (_attributes['drinking'] != null)
                              _buildAttributeChip(
                                  Icons.local_drink, _attributes['drinking']),
                            if (_attributes['smoking'] != null)
                              _buildAttributeChip(
                                  Icons.smoking_rooms, _attributes['smoking']),
                            if (_attributes['education'] != null)
                              _buildAttributeChip(
                                  Icons.school, _attributes['education']),
                            if (_attributes['job'] != null)
                              _buildAttributeChip(
                                  Icons.work, _attributes['job']),
                          ],
                        ),

                        // Footer Actions
                        const SizedBox(height: 40),
                        Center(
                          child: Column(
                            children: [
                              TextButton.icon(
                                  onPressed: () {},
                                  icon: const Icon(Icons.flag,
                                      color: Colors.grey),
                                  label: const Text("Report Account",
                                      style: TextStyle(color: Colors.grey))),
                              if (widget.isMatched) ...[
                                const SizedBox(height: 10),
                                TextButton.icon(
                                    onPressed: () {},
                                    icon: const Icon(Icons.block,
                                        color: Colors.red),
                                    label: const Text("Unmatch",
                                        style: TextStyle(color: Colors.red))),
                              ]
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
          if (_isLoading) const Center(child: CircularProgressIndicator())
        ],
      ),
      // Fixed Bottom Action Bar
      bottomNavigationBar:
          widget.userProfile != null ? null : _buildBottomActionBar(context),
    );
  }

  Widget _buildBottomActionBar(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(40, 5, 40, 10), // Lower buttons
      decoration: BoxDecoration(
        color: Colors.amber.withOpacity(0.1), // Light Orange
        boxShadow: [
          BoxShadow(
              color: Colors.black.withOpacity(0.05),
              blurRadius: 20,
              offset: const Offset(0, -5))
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          // Dislike (Large X)
          _ActionButton(
            icon: Icons.close,
            color: Colors.redAccent,
            size: 60,
            onTap: () => Navigator.pop(context),
          ),
          // Super Like (Small Star)
          _ActionButton(
            icon: Icons.star,
            color: Colors.blueAccent,
            size: 45,
            onTap: () {},
          ),
          // Chat / Boost (Small Bubble)
          _ActionButton(
            icon: Icons.chat_bubble,
            color: Theme.of(context).primaryColor,
            size: 45,
            onTap: () {},
          ),
          // Like (Large Heart)
          _ActionButton(
            icon: Icons.favorite,
            color: Colors.green,
            size: 60,
            onTap: () {},
          ),
        ],
      ),
    );
  }

  Widget _buildAttributeChip(IconData icon, String label) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.grey[300]!),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: Colors.grey[600]),
          const SizedBox(width: 6),
          Text(label,
              style: TextStyle(
                  color: Colors.grey[800], fontWeight: FontWeight.w500)),
        ],
      ),
    );
  }

  String _calculateAge(DateTime dob) {
    final now = DateTime.now();
    int age = now.year - dob.year;
    if (now.month < dob.month || (now.month == dob.month && now.day < dob.day))
      age--;
    return age.toString();
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final Color color;
  final VoidCallback onTap;
  final double size;

  const _ActionButton(
      {required this.icon,
      required this.color,
      required this.onTap,
      this.size = 50});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
            boxShadow: [
              BoxShadow(
                  color: Colors.black.withOpacity(0.1),
                  blurRadius: 10,
                  offset: const Offset(0, 5))
            ]),
        child: Icon(icon, color: color, size: size * 0.5),
      ),
    );
  }
}
