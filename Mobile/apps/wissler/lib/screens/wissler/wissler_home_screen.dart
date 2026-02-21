import 'package:api_client/api_client.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../discover/discover_screen.dart';
import '../discover/user_details_screen.dart';
import '../likes_and_picks/likes_and_picks_screen.dart';
import '../matches_and_chat/matches_and_chat_screen.dart';
import '../profile/user_profile_screen.dart';
import '../profile/edit_profile_screen.dart';
import '../../widgets/custom_bottom_nav.dart';
import '../../widgets/custom_toast.dart';
import 'wissler_screen.dart';

import '../../providers/nav_provider.dart';

// Global Keys
final GlobalKey<WisslerHomeScreenState> wisslerHomeKey = GlobalKey();
final GlobalKey<UserProfileScreenState> userProfileKey = GlobalKey();

class WisslerHomeScreen extends ConsumerStatefulWidget {
  const WisslerHomeScreen({super.key});

  @override
  ConsumerState<WisslerHomeScreen> createState() => WisslerHomeScreenState();
}

class WisslerHomeScreenState extends ConsumerState<WisslerHomeScreen>
    with SingleTickerProviderStateMixin {
  int _currentIndex = 2; // Default to Wissler (Center)
  int _previousIndex = 2; // Track previous tab to return to
  Widget? _subScreen; // Dynamic screen overlay (Edit/Preview)
  DateTime? _lastBackPressTime;

  late AnimationController _menuController;
  late Animation<double> _menuAnimation;

  late List<Widget> _screens;

  @override
  void initState() {
    super.initState();
    _menuController = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 400));
    _menuAnimation = CurvedAnimation(
        parent: _menuController, curve: Curves.easeInOutBack); // Rolling effect

    _screens = [
      const PeopleScreen(key: PageStorageKey('Discover')), // 0: Discover
      const LikesScreen(key: PageStorageKey('Likes')), // 1: Likes
      SwipeScreen(
          key:
              swipeScreenKey), // 2: Wissler Center (Using GlobalKey for Refresh)
      const MatchesScreen(key: PageStorageKey('Chat')), // 3: Chat
      const SizedBox(key: PageStorageKey('More')), // 4: More (Placeholder)
    ];
  }

  // Exposed methods for child widgets
  void openWisslerPlus() {
    _onTabSelected(4);
  }

  void openEditProfile(
      UserProfile profile, List<Country> countries, List<City> cities) {
    if (_menuController.isCompleted) _menuController.reverse();
    setState(() {
      _subScreen = EditProfileScreen(
        userProfile: profile,
        countries: countries,
        cities: cities,
        onBack: () => setState(() => _subScreen = null),
      );
    });
  }

  void openPreviewProfile(UserProfile profile) {
    if (_menuController.isCompleted) _menuController.reverse();
    setState(() {
      _subScreen = UserDetailsScreen(
        userProfile: profile,
        onBack: () => setState(() => _subScreen = null),
      );
    });
  }

  @override
  void dispose() {
    _menuController.dispose();
    super.dispose();
  }

  void setIndex(int index) {
    setState(() {
      _currentIndex = index;
      _subScreen = null; // Clear subscreen on nav change
    });
    ref.read(currentNavIndexProvider.notifier).state = index;
  }

  void _onTabSelected(int index) {
    if (index == 4) {
      // Toggle Menu Animation
      if (_menuController.isCompleted) {
        _menuController.reverse();
        setState(() {
          _currentIndex = _previousIndex;
        });
      } else {
        setState(() {
          _previousIndex = _currentIndex;
          _currentIndex = 4;
          _subScreen = null; // Clear subscreen when opening menu
        });
        ref.read(currentNavIndexProvider.notifier).state = 4;
        _menuController.forward();
        // Refresh Wissler+ Profile when menu opens
        Future.delayed(const Duration(milliseconds: 300), () {
          userProfileKey.currentState?.refresh();
        });
      }
      return;
    }

    // Close menu if open when switching tabs
    if (_menuController.isCompleted || _menuController.isAnimating) {
      _menuController.reverse();
    }

    setState(() {
      _currentIndex = index;
      _subScreen = null; // Clear subscreen when switching tabs
    });
    ref.read(currentNavIndexProvider.notifier).state = index;
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    final menuHeight = size.height * 0.90; // Animate to Top (85%)

    final double safeAreaBottom = MediaQuery.of(context).padding.bottom;

    return WillPopScope(
      onWillPop: () async {
        if (_subScreen != null) {
          setState(() => _subScreen = null);
          return false;
        }
        if (_menuController.isCompleted) {
          _menuController.reverse();
          setState(() {
            _currentIndex = _previousIndex;
          });
          return false;
        }
        if (_currentIndex != 2) {
          setState(() => _currentIndex = 2);
          return false;
        }

        // Double Back to Exit Logic
        final now = DateTime.now();
        if (_lastBackPressTime == null ||
            now.difference(_lastBackPressTime!) > const Duration(seconds: 2)) {
          _lastBackPressTime = now;
          CustomToast.show(context, "Press back again to exit Wissler");
          return false;
        }

        return true; // Exit app
      },
      child: Scaffold(
        resizeToAvoidBottomInset: false, // Handle keyboard externally if needed
        body: Stack(
          children: [
            // 1. Main Content (Back Layer)
            Positioned.fill(
              bottom: _subScreen != null
                  ? 0 // Full height for SubScreen
                  : 65 +
                      safeAreaBottom, // Adjusted for 85px bar height + Safe Area
              child: _subScreen ??
                  IndexedStack(
                    index: _currentIndex == 4 ? _previousIndex : _currentIndex,
                    children: _screens,
                  ),
            ),

            // 3. Custom Navigation Bar (Front Layer - Always on Top)
            // HIDE when _subScreen is active (ie User Details) to prevent covering user actions
            if (_subScreen == null)
              Positioned(
                bottom: 0,
                left: 0,
                right: 0,
                child: CustomBottomNavBar(
                  currentIndex: _currentIndex,
                  onTabSelected: (index) {
                    // Refresh logic for active tab
                    if (index == _currentIndex && index == 2) {
                      // Refresh Wissler Swipe Screen
                      swipeScreenKey.currentState?.loadRecommendations();
                    }
                    _onTabSelected(index);
                  },
                ),
              ),

            // 4. Rolling Menu (Front Layer - Always on Top)
            // Only visible when animating or open to prevent blocking touches when closed
            if (_menuController.isAnimating || _menuController.isCompleted)
              AnimatedBuilder(
                animation: _menuAnimation,
                builder: (context, child) {
                  // Closed State: No peek to avoid covering navbar
                  const double peekHeight = 0;

                  return Positioned(
                    bottom:
                        -menuHeight * (1 - _menuAnimation.value) + peekHeight,
                    left: 0,
                    right: 0,
                    height: menuHeight,
                    child: child!,
                  );
                },
                child: Container(
                  decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: const BorderRadius.vertical(
                          top: Radius.circular(20)), // Small curved edge
                      border: Border.all(
                          color: Colors.grey[300]!,
                          width: 1), // Layered definition
                      boxShadow: [
                        BoxShadow(
                            color: Colors.black.withOpacity(0.1),
                            blurRadius: 10,
                            offset: const Offset(
                                0, -2)) // Upward shadow for layering
                      ]),
                  child: UserProfileScreen(
                    key: userProfileKey,
                    isModal: true,
                    onClose: () {
                      if (_menuController.isCompleted ||
                          _menuController.isAnimating) {
                        _menuController.reverse();
                        setState(() {
                          _currentIndex = _previousIndex;
                        });
                        ref.read(currentNavIndexProvider.notifier).state =
                            _previousIndex;
                      }
                    },
                  ), // Reuse Profile for Rolling Menu
                ),
              ),
          ],
        ),
      ),
    );
  }
}
