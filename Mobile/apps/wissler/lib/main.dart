import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:shared_core/providers/auth_provider.dart';

import 'package:shared_preferences/shared_preferences.dart';

import 'theme/dynamic_theme_provider.dart';
import 'screens/splash_screen.dart';
import 'screens/auth/wissler_login_screen.dart';
import 'screens/auth/wissler_register_screen.dart';
import 'screens/auth/wissler_forgot_password_screen.dart';
import 'screens/home/wissler_home_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final prefs = await SharedPreferences.getInstance();

  runApp(ProviderScope(
    overrides: [
      sharedPreferencesProvider.overrideWithValue(prefs),
    ],
    child: const WisslerApp(),
  ));
}

class WisslerApp extends ConsumerWidget {
  const WisslerApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);
    final currentTheme = ref.watch(dynamicThemeProvider);

    final router = GoRouter(
      initialLocation: '/splash',
      redirect: (context, state) {
        final isLoggedIn = authState.status == AuthStatus.authenticated;
        final isLoggingIn = state.uri.path == '/login' ||
            state.uri.path == '/register' ||
            state.uri.path == '/forgot-password';

        if (!isLoggedIn && !isLoggingIn) return '/login';
        if (isLoggedIn && isLoggingIn) {
          return '/';
        }

        return null;
      },
      routes: [
        GoRoute(
          path: '/',
          builder: (context, state) => const WisslerHomeScreen(),
        ),
        GoRoute(
          path: '/splash',
          builder: (context, state) => const SplashScreen(),
        ),
        GoRoute(
          path: '/login',
          builder: (context, state) => const WisslerLoginScreen(),
        ),
        GoRoute(
          path: '/register',
          builder: (context, state) => const WisslerRegisterScreen(),
        ),
        GoRoute(
          path: '/forgot-password',
          builder: (context, state) => const WisslerForgotPasswordScreen(),
        ),
      ],
    );

    return MaterialApp.router(
      title: 'Wissler',
      theme: currentTheme,
      routerConfig: router,
    );
  }
}
