import 'package:api_client/api_client.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../people/people_screen.dart';
import '../people/user_details_screen.dart';
import '../likes/likes_screen.dart';
import '../matches/matches_screen.dart';
import '../profile/user_profile_screen.dart';
import '../profile/edit_profile_screen.dart';
import '../../widgets/custom_bottom_nav.dart';
import 'swipe_screen.dart';

// Global Key to allow navigation from other screens
final GlobalKey<_WisslerHomeScreenState> wisslerHomeKey = GlobalKey();

class WisslerHomeScreen extends ConsumerStatefulWidget {
  const WisslerHomeScreen({super.key});

  @override
  ConsumerState<WisslerHomeScreen> createState() => _WisslerHomeScreenState();
}

class _WisslerHomeScreenState extends ConsumerState<WisslerHomeScreen>
    with SingleTickerProviderStateMixin {
  int _currentIndex = 2; // Default to Wissler (Center)
  int _previousIndex = 2; // Track previous tab to return to
  Widget? _subScreen; // Dynamic screen overlay (Edit/Preview)

  late AnimationController _menuController;
  late Animation<double> _menuAnimation;

  final List<Widget> _screens = const [
    PeopleScreen(), // 0: Discover
    LikesScreen(), // 1: Likes
    SwipeScreen(), // 2: Wissler Center
    MatchesScreen(), // 3: Chat
    SizedBox(), // 4: More (Placeholder)
  ];

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
  void initState() {
    super.initState();
    _menuController = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 400));
    _menuAnimation = CurvedAnimation(
        parent: _menuController, curve: Curves.easeInOutBack); // Rolling effect
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
        _menuController.forward();
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
        return true; // Exit app
      },
      child: Scaffold(
        resizeToAvoidBottomInset: false, // Handle keyboard externally if needed
        body: Stack(
          children: [
            // 1. Main Content (Back Layer)
            Positioned.fill(
              bottom: 65 +
                  safeAreaBottom, // Adjusted for 85px bar height + Safe Area
              child: _subScreen ??
                  IndexedStack(
                    index: _currentIndex == 4 ? _previousIndex : _currentIndex,
                    children: _screens,
                  ),
            ),

            // 2. Rolling Menu (Middle Layer)
            AnimatedBuilder(
              animation: _menuAnimation,
              builder: (context, child) {
                // Closed State: Peek slightly below visual top to hide
                // Visual Bar height is ~65 (85 total - 20 painter gap).
                // Peek at 55 to ensure overlap.
                const double peekHeight = 55;

                return Positioned(
                  bottom: -menuHeight * (1 - _menuAnimation.value) + peekHeight,
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
                          offset:
                              const Offset(0, -2)) // Upward shadow for layering
                    ]),
                child: UserProfileScreen(
                  isModal: true,
                  onClose: () {
                    if (_menuController.isCompleted ||
                        _menuController.isAnimating) {
                      _menuController.reverse();
                      setState(() {
                        _currentIndex = _previousIndex;
                      });
                    }
                  },
                ), // Reuse Profile for Rolling Menu
              ),
            ),

            // 3. Custom Navigation Bar (Front Layer)
            Positioned(
              bottom: 0,
              left: 0,
              right: 0,
              child: CustomBottomNavBar(
                currentIndex: _currentIndex,
                onTabSelected: _onTabSelected,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
