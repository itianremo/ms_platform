import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:shared_ui/shared_ui.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'theme/dynamic_theme_provider.dart';

import 'screens/home/fitit_home_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final prefs = await SharedPreferences.getInstance();

  runApp(ProviderScope(
    overrides: [
      sharedPreferencesProvider.overrideWithValue(prefs),
    ],
    child: const FitItApp(),
  ));
}

class FitItApp extends ConsumerWidget {
  const FitItApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Watch auth state to redirect
    final authState = ref.watch(authProvider);
    final currentTheme = ref.watch(dynamicThemeProvider);

    final router = GoRouter(
      initialLocation: '/',
      redirect: (context, state) {
        final isLoggedIn = authState.status == AuthStatus.authenticated;
        final isLoggingIn = state.uri.path == '/login' ||
            state.uri.path == '/register' ||
            state.uri.path == '/forgot-password';

        if (!isLoggedIn && !isLoggingIn) return '/login';
        if (isLoggedIn && isLoggingIn) {
          return '/'; // Redirect to home if already logged in
        }

        return null;
      },
      routes: [
        GoRoute(
          path: '/',
          builder: (context, state) => const FitItHomeScreen(),
        ),
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginScreen(),
        ),
        GoRoute(
          path: '/register',
          builder: (context, state) => const RegisterScreen(),
        ),
        GoRoute(
          path: '/forgot-password',
          builder: (context, state) => const ForgotPasswordScreen(),
        ),
      ],
    );

    return MaterialApp.router(
      title: 'FitIt',
      theme: currentTheme,
      routerConfig: router,
    );
  }
}
