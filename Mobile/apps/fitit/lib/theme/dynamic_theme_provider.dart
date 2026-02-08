import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/app_provider.dart';
import 'package:shared_core/providers/auth_provider.dart';
import '../config/constants.dart';
import 'fitit_theme.dart';

class DynamicThemeNotifier extends StateNotifier<ThemeData> {
  final Ref _ref;

  DynamicThemeNotifier(this._ref) : super(FitItTheme.theme) {
    loadTheme();
  }

  Future<void> loadTheme() async {
    final authState = _ref.read(authProvider);

    try {
      if (authState.status == AuthStatus.authenticated) {
        // User is logged in.
        // TODO: Fetch user profile logic if we can get UserId.
        // For now, defaulting to App Config which contains the default theme.
        // If we need user specific theme, we need to implement User Profile fetching.
        await _loadAppConfigDefaults();
      } else {
        // Logged Out
        await _loadAppConfigDefaults();
      }
    } catch (e) {
      print('Error loading theme: $e');
    }
  }

  Future<void> _loadAppConfigDefaults() async {
    final appRepo = _ref.read(appRepositoryProvider);
    try {
      final appConfig = await appRepo.getAppConfig(AppConstants.appId);
      if (appConfig.themeJson != null) {
        _applyThemeFromJson(appConfig.themeJson!);
      }
    } catch (e) {
      print('Failed to load app config: $e');
    }
  }

  void _applyThemeFromJson(String jsonStr) {
    try {
      final Map<String, dynamic> data = json.decode(jsonStr);
      final themeMode = data['theme'] as String?;
      if (themeMode == 'dark') {
        state = FitItTheme.darkTheme;
      } else {
        state = FitItTheme.theme;
      }
    } catch (e) {
      print('Error parsing theme json: $e');
    }
  }
}

final dynamicThemeProvider =
    StateNotifierProvider<DynamicThemeNotifier, ThemeData>((ref) {
  final notifier = DynamicThemeNotifier(ref);
  // Listen to Auth State changes to reload theme
  ref.listen(authProvider, (previous, next) {
    notifier.loadTheme();
  });
  return notifier;
});
