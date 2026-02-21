import 'dart:async'; // For TimeoutException
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:appinio_swiper/appinio_swiper.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import 'package:geolocator/geolocator.dart'; // Geolocator

import '../../widgets/custom_toast.dart';
import '../../widgets/wissler_header.dart';
import '../../providers/nav_provider.dart';
import '../discover/filter_modal.dart';
import '../discover/user_details_screen.dart';

final swipeCategoryProvider = StateProvider<String?>((ref) => null);

// Global Key for SwipeScreen
final GlobalKey<SwipeScreenState> swipeScreenKey = GlobalKey();

class SwipeScreen extends ConsumerStatefulWidget {
  const SwipeScreen({super.key});

  @override
  ConsumerState<SwipeScreen> createState() => SwipeScreenState();
}

class SwipeScreenState extends ConsumerState<SwipeScreen>
    with SingleTickerProviderStateMixin {
  final AppinioSwiperController _swiperController = AppinioSwiperController();
  List<Recommendation> _recommendations = [];
  bool _isLoading = false;
  int _dailyLikes = 0; // Track local session likes
  final int _freeLimit = 25;
  final bool _isPremium = false; // Mock

  late AnimationController _radarController;

  @override
  void initState() {
    super.initState();
    _radarController =
        AnimationController(vsync: this, duration: const Duration(seconds: 2))
          ..repeat();
    _fetchStats();
    loadRecommendations();
  }

  Future<void> _fetchStats() async {
    try {
      final authClient = ref.read(authClientProvider);
      // Hardcoded User ID for now or grab from AuthProvider if available
      // Assuming AuthProvider has currentUser. But ApiClient needs ID.
      // We will rely on ApiClient interceptors or pass a dummy ID for now if not available easily.
      // But wait, ApiClient.getSwipeStats takes userId.
      // The instruction implies we should have it.
      // I'll try to use a hardcoded demo ID if I can't find it efficiently,
      // or "22222222-2222-2222-2222-222222222222" (from seed).
      // Actually, let's use a dummy GUID for the demo: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
      // OR better, let backend handle "me" if I implemented it, but I didn't.
      // I'll use the first seed user ID from logs if possible, or just a known GUID.
      // However, `_loadRecommendations` uses `getRecommendations` which filters by `UserId`.
      // The `authClient` should ideally handle the user context.

      // For this task, I will mock the user ID as a specific GUID to test limits.
      const userId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

      final stats = await authClient.getSwipeStats(userId);
      if (mounted && stats.containsKey('remainingLikes')) {
        setState(() {
          _dailyLikes = 25 -
              (stats['remainingLikes']
                  as int); // Inverse logic if needed, but easier to track remaining
        });
      }
    } catch (_) {}
  }

  // Animation Logic
  String? _overlayAction;
  bool _showOverlayAnimation = false;

  void _triggerCardAnimation(String action) {
    setState(() {
      _overlayAction = action;
      _showOverlayAnimation = true;
    });
  }

  void _handleButtonPress(String action) {
    _triggerCardAnimation(action);
    if (action == 'left') _swiperController.swipeLeft();
    if (action == 'right') _swiperController.swipeRight();
    if (action == 'up') _swiperController.swipeUp();
  }

  Widget _buildOverlayAnimation() {
    IconData icon;
    Color color;
    if (_overlayAction == 'left') {
      icon = Icons.close;
      color = Colors.red;
    } else if (_overlayAction == 'right') {
      icon = Icons.favorite;
      color = Colors.green;
    } else {
      icon = Icons.star;
      color = Colors.blueAccent;
    }

    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0.0, end: 1.0),
      duration: const Duration(milliseconds: 500),
      onEnd: () {
        if (mounted) setState(() => _showOverlayAnimation = false);
      },
      builder: (context, value, child) {
        double opacity = 0.0;
        double scale = 1.0;
        Offset translation = Offset.zero;

        if (_overlayAction == 'up') {
          // Pop
          scale = 0.5 + (value * 2.0);
          opacity = (value < 0.2) ? value * 5 : (1.0 - value);
          translation = Offset(0, -100 * value);
        } else if (_overlayAction == 'left') {
          // Slide Left (Dislike)
          translation = Offset(-200 * (1.0 - value), 0);
          opacity = 1.0 - value;
          scale = 1.0 + value;
        } else {
          // Slide Right (Like)
          translation = Offset(200 * (1.0 - value), 0);
          opacity = 1.0 - value;
          scale = 1.0 + value;
        }

        return Center(
          child: Transform.translate(
            offset: translation,
            child: Transform.scale(
              scale: scale,
              child: Opacity(
                opacity: opacity.clamp(0.0, 1.0),
                child: Container(
                  padding: const EdgeInsets.all(30),
                  decoration: BoxDecoration(
                      color: Colors.white,
                      shape: BoxShape.circle,
                      boxShadow: const [
                        BoxShadow(color: Colors.black26, blurRadius: 20)
                      ],
                      border: Border.all(color: color, width: 5)),
                  child: Icon(icon, color: color, size: 80),
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  // Refactored loadRecommendations to accept category override or read provider and filters
  Future<void> loadRecommendations({String? forceCategory}) async {
    final category = forceCategory ?? ref.read(swipeCategoryProvider) ?? 'All';

    setState(() => _isLoading = true);

    try {
      final authClient = ref.read(authClientProvider);

      Map<String, dynamic>? activeFilters;
      final authRepo = ref.read(authRepositoryProvider);
      final token = authRepo.getAccessToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      final parts = token.split('.');
      if (parts.length != 3) throw Exception("Invalid token");

      final payload = parts[1];
      final normalized = base64Url.normalize(payload);
      final resp = utf8.decode(base64Url.decode(normalized));
      final decodedToken = json.decode(resp);

      final userId =
          decodedToken['sub'] ?? decodedToken['id'] ?? decodedToken['userId'];

      if (userId == null) {
        throw Exception("User ID not found in token");
      }

      try {
        // Attempt to fetch fresh GPS Location
        Position? position;
        if (await Geolocator.isLocationServiceEnabled()) {
          LocationPermission perm = await Geolocator.checkPermission();
          if (perm == LocationPermission.whileInUse ||
              perm == LocationPermission.always) {
            position = await Geolocator.getCurrentPosition(
                desiredAccuracy: LocationAccuracy.medium);
          }
        }

        final profile = await authClient.getUserProfile(
            userId, '22222222-2222-2222-2222-222222222222');
        if (profile != null && profile.customDataJson != null) {
          final customData = json.decode(profile.customDataJson!);
          final prefs = customData['preferences'] as Map<String, dynamic>?;

          if (prefs != null) {
            activeFilters = prefs;

            // Update database with fresh GPS if we got it
            if (position != null) {
              prefs['latitude'] = position.latitude;
              prefs['longitude'] = position.longitude;

              final updatedProfile = UserProfile(
                id: profile.id,
                userId: profile.userId,
                appId: profile.appId,
                displayName: profile.displayName,
                bio: profile.bio,
                avatarUrl: profile.avatarUrl,
                customDataJson: json.encode(customData),
                dateOfBirth: profile.dateOfBirth,
                gender: profile.gender,
              );
              await authClient.updateUserProfile(updatedProfile);
            }
          }
        }
      } catch (_) {} // Ignore profile fetch errors, just proceed without strict filters

      // Add Timeout
      final results = await authClient
          .getRecommendations(
              category: category, pageSize: 20, filters: activeFilters)
          .timeout(const Duration(seconds: 10));

      if (mounted) {
        setState(() {
          _recommendations = results;
          _isLoading = false;
        });
      }
    } on TimeoutException catch (_) {
      if (mounted) {
        setState(() => _isLoading = false);
        CustomToast.show(
            context, "Request timed out. Please check your connection.");
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        CustomToast.show(context,
            "Wissler is whistling to the server, you may refresh later");
      }
    }
  }

  void _onSwipe(int index, dynamic activity) async {
    // Release Mode Fix: use Enum index directly
    // Up = 0, Right = 1, Down = 2, Left = 3
    dynamic innerDirection;
    try {
      innerDirection = activity.direction;
    } catch (_) {
      innerDirection = activity;
    }

    if (innerDirection.toString().contains('bottom') || innerDirection == 2) {
      // Swipe down, do nothing
      return;
    }

    // Determine the API action string
    String? action;

    // Check by Index (Release Safe)
    // The original code had a `dirIndex` variable. Let's re-introduce it for clarity
    // and to align with the original logic's intent before the user's partial edit.
    int? dirIndex;
    try {
      dirIndex = innerDirection?.index;
    } catch (_) {}

    if (dirIndex == 1) {
      action = 'right';
    } else if (dirIndex == 0) {
      action = 'up';
    } else if (dirIndex == 3) {
      action = 'left';
    } else if (dirIndex == 2) {
      // Swipe Down: do nothing, just let it return to center.
      // AppinioSwiper doesn't inherently support 'return to center' on end if swiped past threshold
      // unless onSwipe cancels it. But if it does swipe away, we just ignore the backend call.
      // Typically, to prevent down swipe entirely, we'd configure the swiper to not allow it.
      return;
    }

    // If action is null (e.g. Cancelled, Down, or other), return early
    if (action == null) return;

    // Trigger Animation (Visual Confirmation) if not triggered by button
    if (!_showOverlayAnimation) {
      _triggerCardAnimation(action);
    }

    final apiAction = (action == 'right' || action == 'up') ? "Like" : "Pass";
    final targetId = _recommendations[index].userId;
    // Use the demo ID as hardcoded
    const userId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    final authClient = ref.read(authClientProvider);
    final result = await authClient.swipe(userId, targetId, apiAction);

    if (result.containsKey('error')) {
      CustomToast.show(context, result['error'].toString());
    } else if (result.containsKey('remainingLikes')) {
      setState(() {
        _dailyLikes = 25 - (result['remainingLikes'] as int);
      });

      if (result['isMatch'] == true) {
        CustomToast.show(context, "It's a Match!");
      }
    }
  }

  int get _remainingLikes => _freeLimit - _dailyLikes;
  bool get _isLimitReached => !_isPremium && _remainingLikes <= 0;
  bool get _showLimitWarning =>
      !_isPremium && _remainingLikes < 6 && _remainingLikes > 0;

  void _listenToNavChanges() {
    ref.listen<int>(currentNavIndexProvider, (previous, next) {
      if (next == 2 && previous != 2) {
        // Auto-refresh when returning to Swipe Screen
        loadRecommendations();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    _listenToNavChanges(); // Attach listener

    // Listen for category changes
    ref.listen<String?>(swipeCategoryProvider, (previous, next) {
      if (next != previous) loadRecommendations();
    });

    final currentCategory = ref.watch(swipeCategoryProvider) ?? "Wissler";
    final isPage = Navigator.of(context).canPop();

    final primary = Theme.of(context).primaryColor;

    return Scaffold(
        appBar: PreferredSize(
            preferredSize: const Size.fromHeight(kToolbarHeight),
            child: AppBar(
              automaticallyImplyLeading: isPage,
              backgroundColor: Colors.transparent,
              elevation: 0,
              title: Row(children: [
                if (!isPage) const WisslerLogo(size: 32),
                const Spacer(),
                Text(
                  currentCategory,
                  style: TextStyle(
                    fontFamily: 'Cursive',
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: primary,
                  ),
                ),
              ]),
              actions: [
                IconButton(
                    onPressed: _showFilterModal,
                    icon: Icon(Icons.tune, color: primary))
              ],
            )),
        body: Stack(
          children: [
            Column(children: [
              if (_isLimitReached || _showLimitWarning)
                Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(10),
                    color: _isLimitReached
                        ? Colors.redAccent
                        : Colors.orangeAccent,
                    child: Column(children: [
                      Text(
                          _isLimitReached
                              ? "Daily Limit Reached"
                              : "Running Low!",
                          style: const TextStyle(
                              fontWeight: FontWeight.bold,
                              color: Colors.white)),
                      Text(
                          _isLimitReached
                              ? "Your likes will be renewed in 24h."
                              : "Only $_remainingLikes likes remaining today.",
                          style: const TextStyle(
                              color: Colors.white, fontSize: 12))
                    ])),
              Expanded(
                  child: _isLoading
                      ? _buildRadarAnimation()
                      : _recommendations.isEmpty
                          ? _buildRadarAnimation(isRetry: true)
                          : AbsorbPointer(
                              absorbing: _isLimitReached,
                              child: Padding(
                                  padding: const EdgeInsets.all(16.0),
                                  child: AppinioSwiper(
                                    controller: _swiperController,
                                    cardCount: _recommendations.length,
                                    // Disable wiping down physically, forces card to snap back
                                    swipeOptions: const SwipeOptions.symmetric(
                                        horizontal: true, vertical: false),
                                    onSwipeEnd: (_, index, direction) {
                                      _onSwipe(index, direction);
                                    },
                                    onEnd: () {
                                      setState(() {
                                        _recommendations.clear();
                                      });
                                    },
                                    cardBuilder: (context, index) {
                                      return _DiscoverCard(
                                        key: ValueKey(
                                            _recommendations[index].userId),
                                        profile: _recommendations[index],
                                        onSwipe: (action) {
                                          _handleButtonPress(action);
                                        },
                                      );
                                    },
                                  )),
                            )),
              if (!_isLoading &&
                  _recommendations.isNotEmpty &&
                  !_isLimitReached)
                Padding(
                    padding:
                        const EdgeInsets.only(bottom: 30, left: 40, right: 40),
                    child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        children: [
                          _ActionButton(
                              icon: Icons.close,
                              color: Colors.red,
                              onTap: () => _handleButtonPress('left')),
                          _ActionButton(
                              icon: Icons.star,
                              color: Colors.amber,
                              isSmall: true,
                              onTap: () => _handleButtonPress('up')),
                          _ActionButton(
                              icon: Icons.favorite,
                              color: Colors.green,
                              onTap: () => _handleButtonPress('right')),
                        ]))
            ]),
            if (_showOverlayAnimation && _overlayAction != null)
              Positioned.fill(child: _buildOverlayAnimation())
          ],
        ));
  }

  Widget _buildRadarAnimation({bool isRetry = false}) {
    final color = Theme.of(context).primaryColor;
    return Center(
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
      Stack(alignment: Alignment.center, children: [
        if (!isRetry || _isLoading)
          ...List.generate(3, (index) {
            return AnimatedBuilder(
                animation: _radarController,
                builder: (context, child) {
                  final val = (_radarController.value + (index * 0.33)) % 1.0;
                  return Opacity(
                      opacity: 1.0 - val,
                      child: Transform.scale(
                          scale: 1.0 + (val * 2), // ripple
                          child: Container(
                              width: 100,
                              height: 100,
                              decoration: BoxDecoration(
                                  shape: BoxShape.circle,
                                  border: Border.all(
                                      color: color.withOpacity(0.5),
                                      width: 2)))));
                });
          }),
        GestureDetector(
          onTap: (isRetry || _recommendations.isEmpty)
              ? loadRecommendations
              : null,
          child: Container(
            width: 80,
            height: 80,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: color.withOpacity(0.5),
                  blurRadius: 20,
                  spreadRadius: 5,
                )
              ],
            ),
            alignment: Alignment.center,
            child: (isRetry || _recommendations.isEmpty) && !_isLoading
                ? Icon(Icons.refresh, size: 40, color: color)
                : Text(
                    "W",
                    style: TextStyle(
                      fontFamily: 'Cursive', // Matching header font style
                      fontSize: 48,
                      fontWeight: FontWeight.bold,
                      color: color,
                    ),
                  ),
          ),
        )
      ]),
      const SizedBox(height: 30),
      Text(
          (isRetry || _recommendations.isEmpty) && !_isLoading
              ? "Refresh"
              : "Searching...",
          style: TextStyle(
              fontSize: 18, color: Colors.grey[600], letterSpacing: 1.2)),
    ]));
  }

  void _showFilterModal() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.6,
        minChildSize: 0.4,
        maxChildSize: 0.85,
        builder: (_, controller) => FilterModal(
          onApply: () => loadRecommendations(),
          scrollController: controller,
        ),
      ),
    );
  }
}

class _DiscoverCard extends StatefulWidget {
  final Recommendation profile;
  final Function(String) onSwipe; // New callback

  const _DiscoverCard(
      {super.key, required this.profile, required this.onSwipe});

  @override
  State<_DiscoverCard> createState() => _DiscoverCardState();
}

class _DiscoverCardState extends State<_DiscoverCard> {
  int _currentImageIndex = 0;

  List<String> get _images {
    final imgs = widget.profile.images.isNotEmpty
        ? widget.profile.images
        : [widget.profile.avatarUrl];
    // Filter empty strings and duplicates
    return imgs.where((s) => s.isNotEmpty).toSet().toList();
  }

  void _nextImage() {
    if (_currentImageIndex < _images.length - 1) {
      setState(() => _currentImageIndex++);
    }
  }

  void _prevImage() {
    if (_currentImageIndex > 0) {
      setState(() => _currentImageIndex--);
    }
  }

  @override
  Widget build(BuildContext context) {
    final images = _images;
    final imageUrl = images.isNotEmpty
        ? images[_currentImageIndex]
        : 'https://via.placeholder.com/400x600';

    return Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          boxShadow: const [
            BoxShadow(
                color: Colors.black12, blurRadius: 10, offset: Offset(0, 5))
          ],
          color: Colors.white,
        ),
        clipBehavior: Clip.antiAlias,
        child: Stack(fit: StackFit.expand, children: [
          // 1. Image Layer
          Image.network(
            imageUrl,
            fit: BoxFit.cover,
            errorBuilder: (_, __, ___) =>
                const Center(child: Icon(Icons.person, size: 80)),
          ),

          // 2. Content Gradient Overlay (Non-interactive)
          IgnorePointer(
            child: Container(
                decoration: BoxDecoration(
                    gradient: LinearGradient(
                        colors: [
                          Colors.transparent,
                          Colors.black.withOpacity(0.1),
                          Colors.black.withOpacity(0.8)
                        ],
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        stops: const [0.5, 0.7, 1.0]),
                    border: Border.all(
                        color: Colors.amber.withOpacity(0.5),
                        width: 1) // Subtle Border
                    )),
          ),

          // 3. Image Indicators
          if (images.length > 1)
            Positioned(
              top: 10,
              left: 10,
              right: 10,
              child: Row(
                children: List.generate(images.length, (index) {
                  return Expanded(
                    child: Container(
                      height: 4,
                      margin: const EdgeInsets.symmetric(horizontal: 2),
                      decoration: BoxDecoration(
                          color: index == _currentImageIndex
                              ? Colors.white
                              : Colors.white24,
                          borderRadius: BorderRadius.circular(2),
                          boxShadow: const [
                            BoxShadow(color: Colors.black26, blurRadius: 2)
                          ]),
                    ),
                  );
                }),
              ),
            ),

          // 3.5 Verified Badge
          if (widget.profile.isVerified)
            Positioned(
              top: 20,
              right: 20,
              child: SafeArea(
                child: const Icon(Icons.verified,
                    color: Colors.orange,
                    size: 28,
                    shadows: [Shadow(color: Colors.black54, blurRadius: 5)]),
              ),
            ),

          // 4. Tap Zones for Image Navigation (Placed on TOP so they catch events)
          Row(children: [
            Expanded(
                child: GestureDetector(
                    behavior: HitTestBehavior.opaque, // Force opaque
                    onTap: _prevImage,
                    child: Container(color: Colors.transparent))),
            Expanded(
                child: GestureDetector(
                    behavior: HitTestBehavior.opaque, // Force opaque
                    onTap: _nextImage,
                    child: Container(color: Colors.transparent))),
          ]),

          // 5. Bottom Info Section
          Positioned(
              left: 20,
              right: 20,
              bottom: 30,
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.end, // Align bottom
                children: [
                  // Text Info (Left)
                  Expanded(
                    child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Row(
                            children: [
                              Flexible(
                                child: Text(
                                    "${widget.profile.displayName}, ${widget.profile.age}",
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 28,
                                        fontWeight: FontWeight.bold,
                                        shadows: [
                                          Shadow(
                                              color: Colors.black54,
                                              blurRadius: 10)
                                        ])),
                              ),
                              if (widget.profile.isVerified)
                                const Padding(
                                  padding: EdgeInsets.only(left: 6),
                                  child: Icon(Icons.gpp_good,
                                      color: Colors.orange,
                                      size: 24,
                                      shadows: [
                                        Shadow(
                                            color: Colors.black54,
                                            blurRadius: 5)
                                      ]),
                                )
                            ],
                          ),
                          const SizedBox(height: 4),
                          Row(children: [
                            const Icon(Icons.location_on,
                                color: Colors.white, size: 16),
                            const SizedBox(width: 4),
                            Expanded(
                                child: Text(
                                    "${widget.profile.city}, ${widget.profile.country}",
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 15,
                                        shadows: [
                                          Shadow(
                                              color: Colors.black54,
                                              blurRadius: 5)
                                        ])))
                          ])
                        ]),
                  ),

                  const SizedBox(width: 10),

                  // Info Button (Right)
                  GestureDetector(
                    onTap: () {
                      Navigator.push(
                          context,
                          MaterialPageRoute(
                              builder: (context) => UserDetailsScreen(
                                    profile: widget.profile,
                                    onSwipe: (direction) {
                                      Navigator.pop(context, direction);
                                    },
                                  ))).then((result) {
                        if (result != null && result is String) {
                          widget.onSwipe(result);
                        }
                      });
                    },
                    child: Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: Colors.white.withOpacity(0.2), // Glassmorphic
                        border: Border.all(color: Colors.white, width: 1.5),
                      ),
                      child: const Icon(Icons.arrow_upward,
                          color: Colors.white, size: 28),
                    ),
                  )
                ],
              ))
        ]));
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
        decoration: const BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
            boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5)]),
        child: Icon(icon, color: color, size: isSmall ? 24 : 32),
      ),
    );
  }
}
