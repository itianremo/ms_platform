import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:appinio_swiper/appinio_swiper.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import '../../widgets/wissler_header.dart';
import '../people/filter_modal.dart';
import '../home/wissler_home_screen.dart'; // For wisslerHomeKey

final swipeCategoryProvider = StateProvider<String?>((ref) => null);

class SwipeScreen extends ConsumerStatefulWidget {
  const SwipeScreen({super.key}); // Remove Category param, use Provider

  @override
  ConsumerState<SwipeScreen> createState() => _SwipeScreenState();
}

class _SwipeScreenState extends ConsumerState<SwipeScreen>
    with SingleTickerProviderStateMixin {
  final AppinioSwiperController _swiperController = AppinioSwiperController();
  List<Recommendation> _recommendations = [];
  bool _isLoading = false;
  int _dailyLikes = 0; // Track local session likes
  final int _freeLimit = 25;
  bool _isPremium = false; // Mock

  late AnimationController _radarController;

  @override
  void initState() {
    super.initState();
    _radarController =
        AnimationController(vsync: this, duration: const Duration(seconds: 2))
          ..repeat();
    // Load initial (All) or wait for provider?
    // Provider defaults to null (All).
    // usage of ref.read in initState is restricted, typically do in didChangeDependencies or just build.
    // But we need to load ONCE or on change.

    // Actually, asking for "onInit" load is fine, but we also need listener.
    // Let's do it in build via .listen or useEffect logic manually.
  }

  // Refactored _loadRecommendations to accept category override or read provider
  Future<void> _loadRecommendations({String? forceCategory}) async {
    final category = forceCategory ?? ref.read(swipeCategoryProvider) ?? 'All';

    setState(() => _isLoading = true);
    await Future.delayed(const Duration(seconds: 1)); // UX delay for radar
    try {
      final authClient = ref.read(authClientProvider);
      final results =
          await authClient.getRecommendations(category: category, pageSize: 20);
      if (mounted) {
        setState(() {
          _recommendations = results;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _onSwipe(int index, dynamic direction) {
    // Handling direction safely as dynamic due to version mismatch
    final dirStr = direction.toString();
    if (dirStr.contains('right') || dirStr.contains('top')) {
      setState(() {
        _dailyLikes++;
      });
    }
  }

  bool get _isLimitReached => !_isPremium && _dailyLikes >= _freeLimit;

  @override
  Widget build(BuildContext context) {
    // Listen for category changes
    ref.listen<String?>(swipeCategoryProvider, (previous, next) {
      if (next != previous) _loadRecommendations();
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
        body: Column(children: [
          if (_isLimitReached)
            Container(
                width: double.infinity,
                padding: const EdgeInsets.all(10),
                color: Colors.amber,
                child: const Column(children: [
                  Text("Daily Limit Reached",
                      style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF003366))),
                  Text("Your likes will be renewed in 24h.",
                      style: TextStyle(color: Color(0xFF003366), fontSize: 12))
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
                                cardBuilder: (context, index) => _DiscoverCard(
                                    profile: _recommendations[index]),
                                onSwipeEnd: (_, index, direction) {
                                  _onSwipe(index, direction);
                                },
                                onEnd: () {
                                  // Infinite Loop: Shuffle and Restart
                                  setState(() {
                                    _recommendations.shuffle();
                                    // Hack to force key refresh or just rely on state update
                                    // With AppinioSwiper we might need to reset controller or just re-render
                                    // If cardCount update isn't enough, we might need a unique key for the swiper
                                  });
                                },
                                // loop: true parameter exists in some versions, but user asked for "shuffle"
                                // so manual shuffle onEnd is safer.
                              )),
                        )),
          if (!_isLoading && _recommendations.isNotEmpty && !_isLimitReached)
            Padding(
                padding: const EdgeInsets.only(bottom: 30, left: 40, right: 40),
                child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: [
                      _ActionButton(
                          icon: Icons.close,
                          color: Colors.red,
                          onTap: () => _swiperController.swipeLeft()),
                      _ActionButton(
                          icon: Icons.star,
                          color: Colors.amber,
                          isSmall: true,
                          // Gold Button -> Open Wissler+ (More Menu)
                          onTap: () =>
                              wisslerHomeKey.currentState?.openWisslerPlus()),
                      _ActionButton(
                          icon: Icons.favorite,
                          color: Colors.green,
                          onTap: () => _swiperController.swipeRight()),
                    ]))
        ]));
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
          onTap: isRetry ? _loadRecommendations : null,
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
            child: isRetry && !_isLoading
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
      Text(isRetry && !_isLoading ? "No profiles found." : "Searching...",
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
        builder: (_, controller) => Container(
            decoration: const BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
            child: SingleChildScrollView(
              controller: controller,
              child: FilterModal(
                onApply: (minAge, maxAge, country) => _loadRecommendations(),
              ),
            )),
      ),
    );
  }
}

class _DiscoverCard extends StatelessWidget {
  final Recommendation profile;
  const _DiscoverCard({required this.profile});

  @override
  Widget build(BuildContext context) {
    return Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
                color: Colors.black12,
                blurRadius: 10,
                offset: const Offset(0, 5))
          ],
          color: Colors.white,
        ),
        clipBehavior: Clip.antiAlias,
        child: Stack(fit: StackFit.expand, children: [
          Image.network(
            profile.avatarUrl.isNotEmpty
                ? profile.avatarUrl
                : 'https://via.placeholder.com/400x600',
            fit: BoxFit.cover,
            errorBuilder: (_, __, ___) =>
                const Center(child: Icon(Icons.person, size: 80)),
          ),
          Container(
              decoration: BoxDecoration(
                  gradient: LinearGradient(
                      colors: [
                        Colors.transparent,
                        Colors.black.withOpacity(0.8)
                      ],
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      stops: const [0.6, 1.0]),
                  border:
                      Border.all(color: Colors.amber, width: 2) // Gold Border
                  )),
          Positioned(
              left: 20,
              bottom: 30,
              child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text("${profile.displayName}, ${profile.age}",
                        style: const TextStyle(
                            color: Colors.white,
                            fontSize: 28,
                            fontWeight: FontWeight.bold)),
                    Row(children: [
                      const Icon(Icons.location_on,
                          color: Colors.white70, size: 16),
                      Text("${profile.city}, ${profile.country}",
                          style: const TextStyle(color: Colors.white70))
                    ])
                  ]))
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
        decoration: BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
            boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5)]),
        child: Icon(icon, color: color, size: isSmall ? 24 : 32),
      ),
    );
  }
}
