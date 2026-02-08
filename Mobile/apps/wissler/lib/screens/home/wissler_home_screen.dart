import 'package:flutter/material.dart';

import '../matching/matching_screen.dart';
import '../matches/matches_screen.dart';
import '../profile/user_profile_screen.dart';
import '../likes/likes_screen.dart';
import '../people/people_screen.dart';

class WisslerHomeScreen extends StatefulWidget {
  const WisslerHomeScreen({super.key});

  @override
  State<WisslerHomeScreen> createState() => _WisslerHomeScreenState();
}

class _WisslerHomeScreenState extends State<WisslerHomeScreen> {
  int _selectedIndex = 0; // Default to Discover (index 0)

  static const List<Widget> _widgetOptions = <Widget>[
    MatchingScreen(), // 0: Discover
    PeopleScreen(), // 1: People
    MatchesScreen(), // 2: Chat (Matches) - Middle
    LikesScreen(), // 3: Likes
    UserProfileScreen(), // 4: Profile
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return Scaffold(
      body: IndexedStack(
        // Use IndexedStack to preserve state
        index: _selectedIndex,
        children: _widgetOptions,
      ),
      bottomNavigationBar: NavigationBarTheme(
        data: NavigationBarThemeData(
          labelTextStyle: WidgetStateProperty.all(
            const TextStyle(fontSize: 11, fontWeight: FontWeight.w500),
          ),
        ),
        child: NavigationBar(
          selectedIndex: _selectedIndex,
          onDestinationSelected: _onItemTapped,
          backgroundColor: isDark ? const Color(0xFF0F172A) : Colors.white,
          indicatorColor: theme.primaryColor.withOpacity(0.15),
          height: 70,
          destinations: <NavigationDestination>[
            const NavigationDestination(
              icon: Icon(Icons.style_outlined),
              selectedIcon: Icon(Icons.style_rounded),
              label: 'Discover',
            ),
            const NavigationDestination(
              icon: Icon(Icons.people_outline_rounded),
              selectedIcon: Icon(Icons.people_rounded),
              label: 'People',
            ),
            NavigationDestination(
              icon: const Icon(Icons.chat_bubble_outline_rounded, size: 32),
              selectedIcon: Icon(Icons.chat_bubble_rounded,
                  size: 32, color: theme.primaryColor),
              label: 'Chat',
            ),
            const NavigationDestination(
              icon: Icon(Icons.favorite_outline_rounded),
              selectedIcon: Icon(Icons.favorite_rounded),
              label: 'Likes',
            ),
            const NavigationDestination(
              icon: Icon(Icons.person_outline_rounded),
              selectedIcon: Icon(Icons.person_rounded),
              label: 'Profile',
            ),
          ],
        ),
      ),
    );
  }
}
