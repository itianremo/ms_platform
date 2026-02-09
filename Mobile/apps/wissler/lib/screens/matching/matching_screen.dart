import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:appinio_swiper/appinio_swiper.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';

class MatchingScreen extends ConsumerStatefulWidget {
  const MatchingScreen({super.key});

  @override
  ConsumerState<MatchingScreen> createState() => _MatchingScreenState();
}

class _MatchingScreenState extends ConsumerState<MatchingScreen> {
  final AppinioSwiperController _swiperController = AppinioSwiperController();
  List<UserProfile> _profiles = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadProfiles();
  }

  Future<void> _loadProfiles() async {
    try {
      final authClient = ref.read(authClientProvider);
      final repo = ref.read(authRepositoryProvider);
      final token = repo.getAccessToken();

      String? currentUserId;
      if (token != null) {
        final payload = _decodeJwt(token);
        currentUserId = payload['sub'] ?? payload['id'] ?? payload['userId'];
      }

      // Hardcoded Wissler AppID
      final profiles =
          await authClient.getProfiles('22222222-2222-2222-2222-222222222222');

      if (mounted) {
        setState(() {
          // Filter out current user and invalid profiles
          _profiles = profiles.where((p) => p.userId != currentUserId).toList();
          _isLoading = false;
        });
      }
    } catch (e) {
      print('Error loading profiles: $e');
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Map<String, dynamic> _decodeJwt(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return {};
      final payload = _decodeBase64(parts[1]);
      return json.decode(payload);
    } catch (e) {
      return {};
    }
  }

  String _decodeBase64(String str) {
    String output = str.replaceAll('-', '+').replaceAll('_', '/');
    switch (output.length % 4) {
      case 0:
        break;
      case 2:
        output += '==';
        break;
      case 3:
        output += '=';
        break;
    }
    return utf8.decode(base64Url.decode(output));
  }

  void _showProfileDetails(UserProfile profile) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => _ProfileDetailModal(profile: profile),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_profiles.isEmpty) {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.style_outlined, size: 64, color: Colors.grey),
            SizedBox(height: 16),
            Text("No more profiles to explore!",
                style: TextStyle(color: Colors.grey)),
          ],
        ),
      );
    }

    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            Expanded(
              child: AppinioSwiper(
                controller: _swiperController,
                cardCount: _profiles.length,
                onSwipeEnd: (previousIndex, targetIndex, activity) {
                  // Handle swipe
                },
                cardBuilder: (context, index) {
                  final profile = _profiles[index];
                  return _ProfileCard(
                    profile: profile,
                    onTap: () => _showProfileDetails(profile),
                  );
                },
              ),
            ),
            const SizedBox(height: 24),
            // Action Buttons
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 20),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  _ActionButton(
                    icon: Icons.close_rounded,
                    color: Colors.red,
                    onTap: () => _swiperController.swipeLeft(),
                  ),
                  _ActionButton(
                    icon: Icons.star_rounded,
                    color: Colors.blue,
                    isSmall: true,
                    onTap: () {}, // Super like?
                  ),
                  _ActionButton(
                    icon: Icons.favorite_rounded,
                    color: Colors.green,
                    onTap: () => _swiperController.swipeRight(),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ProfileCard extends StatelessWidget {
  final UserProfile profile;
  final VoidCallback onTap;

  const _ProfileCard({required this.profile, required this.onTap});

  String _getImageUrl() {
    // Try to get first photo from customData, else avatarUrl
    try {
      if (profile.customDataJson != null) {
        final data = jsonDecode(profile.customDataJson!);
        if (data['photos'] != null && (data['photos'] as List).isNotEmpty) {
          return data['photos'][0];
        }
      }
    } catch (_) {}
    return profile.avatarUrl ??
        'https://via.placeholder.com/400x600?text=No+Image';
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withOpacity(0.1),
                blurRadius: 10,
                offset: const Offset(0, 5))
          ],
        ),
        clipBehavior: Clip.antiAlias,
        child: Stack(
          fit: StackFit.expand,
          children: [
            // Image
            Image.network(
              _getImageUrl(),
              fit: BoxFit.cover,
              errorBuilder: (_, __, ___) => Container(
                  color: Colors.grey[300],
                  child:
                      const Icon(Icons.person, size: 64, color: Colors.grey)),
            ),
            // Gradient Overlay
            Container(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [
                    Colors.transparent,
                    Colors.black.withOpacity(0.8),
                  ],
                  stops: const [0.6, 1.0],
                ),
              ),
            ),
            // Text Info
            Positioned(
              bottom: 20,
              left: 20,
              right: 20,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${profile.displayName}, ${_calculateAge(profile.dateOfBirth)}',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 28,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    profile.bio ?? 'No bio available',
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: Colors.white70,
                      fontSize: 16,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  int _calculateAge(DateTime? dob) {
    if (dob == null) return 25; // Default age if unknown
    final now = DateTime.now();
    int age = now.year - dob.year;
    if (now.month < dob.month ||
        (now.month == dob.month && now.day < dob.day)) {
      age--;
    }
    return age;
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final Color color;
  final VoidCallback onTap;
  final bool isSmall;

  const _ActionButton(
      {required this.icon,
      required this.color,
      required this.onTap,
      this.isSmall = false});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: EdgeInsets.all(isSmall ? 12 : 16),
        decoration: BoxDecoration(
          color: Colors.white,
          shape: BoxShape.circle,
          boxShadow: [
            BoxShadow(
                color: Colors.black.withOpacity(0.1),
                blurRadius: 10,
                offset: const Offset(0, 5))
          ],
        ),
        child: Icon(icon, color: color, size: isSmall ? 24 : 32),
      ),
    );
  }
}

class _ProfileDetailModal extends StatelessWidget {
  final UserProfile profile;

  const _ProfileDetailModal({required this.profile});

  List<String> _getAllPhotos() {
    List<String> photos = [];
    try {
      if (profile.customDataJson != null) {
        final data = jsonDecode(profile.customDataJson!);
        if (data['photos'] != null) {
          photos = List<String>.from(data['photos']);
        }
      }
    } catch (_) {}

    if (photos.isEmpty && profile.avatarUrl != null) {
      photos.add(profile.avatarUrl!);
    }
    return photos;
  }

  @override
  Widget build(BuildContext context) {
    final photos = _getAllPhotos();
    final theme = Theme.of(context);

    return DraggableScrollableSheet(
      initialChildSize: 0.85,
      minChildSize: 0.5,
      maxChildSize: 0.95,
      builder: (_, controller) {
        return Container(
          decoration: BoxDecoration(
            color: theme.scaffoldBackgroundColor,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: Column(
            children: [
              // Handle
              Container(
                width: 40,
                height: 4,
                margin: const EdgeInsets.symmetric(vertical: 12),
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Expanded(
                child: ListView(
                  controller: controller,
                  padding: EdgeInsets.zero,
                  children: [
                    // Photos Carousel
                    SizedBox(
                      height: 400,
                      child: PageView.builder(
                        itemCount: photos.length,
                        itemBuilder: (context, index) {
                          return Image.network(
                            photos[index],
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => Container(
                                color: Colors.grey[300],
                                child: const Icon(Icons.broken_image)),
                          );
                        },
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.all(24),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            profile.displayName ?? 'Unknown',
                            style: const TextStyle(
                                fontSize: 32, fontWeight: FontWeight.bold),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            profile.bio ?? '',
                            style: const TextStyle(
                                fontSize: 18, height: 1.5, color: Colors.grey),
                          ),
                          const SizedBox(height: 32),
                          // Custom Data Info
                          _buildInfoSection(profile.customDataJson),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildInfoSection(String? jsonStr) {
    if (jsonStr == null) return const SizedBox.shrink();
    try {
      final data = jsonDecode(jsonStr);
      final city = data['city'];
      final country = data['country'];
      final interests = data['interests'];

      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (city != null || country != null)
            ListTile(
              leading: const Icon(Icons.location_on_outlined),
              title: Text([city, country]
                  .where((s) => s != null && s.isNotEmpty)
                  .join(', ')),
            ),
          if (interests != null)
            ListTile(
              leading: const Icon(Icons.favorite_border),
              title: Text(interests.toString()),
            ),
        ],
      );
    } catch (_) {
      return const SizedBox.shrink();
    }
  }
}
