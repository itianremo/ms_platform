import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class SplashScreen extends ConsumerStatefulWidget {
  const SplashScreen({super.key});

  @override
  ConsumerState<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends ConsumerState<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    // Artificial delay for branding
    await Future.delayed(const Duration(seconds: 2));

    if (!mounted) return;

    // The router redirection logic in main.dart usually handles this,
    // but sometimes a splash screen is manually navigating.
    // However, since we use GoRouter 'redirect', we might just need to poke the state
    // or simply let the router do its job.
    // IF the router is watching authProvider, it will redirect automatically once the state is determined.

    // In our main.dart, we are watching authState.
    // So actually, this Splash screen might just be a visual holder while the async auth check happens.
    // But since we added a delay, we might need to manually trigger nav if state is already 'unknown'.

    // For simplicity with GoRouter redirect:
    // We can just go to '/' and let the redirect interceptor decide where to send us (Home or Login).
    context.go('/');
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      backgroundColor: theme.primaryColor,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Logo
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.1),
                    blurRadius: 10,
                    spreadRadius: 2,
                  )
                ],
              ),
              child: Icon(
                Icons.volunteer_activism, // Heart-like icon
                size: 60,
                color: theme.primaryColor,
              ),
            ),
            const SizedBox(height: 24),
            // App Name
            const Text(
              'Wissler',
              style: TextStyle(
                fontSize: 32,
                fontWeight: FontWeight.bold,
                color: Colors.white,
                letterSpacing: 1.2,
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              'Find your match',
              style: TextStyle(
                fontSize: 16,
                color: Colors.white70,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
