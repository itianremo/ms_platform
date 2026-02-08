import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/app_provider.dart';
import 'package:shared_core/providers/auth_provider.dart';
import '../config/constants.dart';
import 'wissler_theme.dart';

class DynamicThemeNotifier extends StateNotifier<ThemeData> {
  final Ref _ref;
  Color _currentPrimary = WisslerPalette.coral; // Default cheerful color
  bool _isDark = false;

  DynamicThemeNotifier(this._ref)
      : super(WisslerTheme.getTheme(
            primary: WisslerPalette.coral, isDark: false)) {
    loadTheme();
  }

  Future<void> loadTheme() async {
    final authState = _ref.read(authProvider);

    try {
      if (authState.status == AuthStatus.authenticated) {
        await _loadAppConfigDefaults();
      } else {
        // Default if validation fails or unauthenticated
        _updateTheme();
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

  void _applyThemeFromJson(String? jsonStr) {
    if (jsonStr == null || jsonStr.isEmpty) return;
    try {
      if (!jsonStr.trim().startsWith('{')) {
        print('Invalid JSON format');
        return;
      }
      final Map<String, dynamic> data = json.decode(jsonStr);

      final themeMode = data['theme'] as String?;
      if (themeMode == 'dark') {
        _isDark = true;
      } else {
        _isDark = false;
      }

      final colorValue = data['primaryColor'] as int?;
      if (colorValue != null) {
        _currentPrimary = Color(colorValue);
      }

      _updateTheme();
    } catch (e) {
      print('Error parsing theme json: $e');
      _updateTheme(); // Fallback
    }
  }

  void toggleDarkMode(bool isDark) {
    _isDark = isDark;
    _updateTheme();
  }

  void setPrimaryColor(Color color) {
    _currentPrimary = color;
    _updateTheme();
  }

  // Deprecated usage support if called directly
  void setTheme(ThemeData theme) {
    // Try to infer brightness
    _isDark = theme.brightness == Brightness.dark;
    state = theme;
  }

  void _updateTheme() {
    state = WisslerTheme.getTheme(primary: _currentPrimary, isDark: _isDark);
  }

  Color get currentPrimary => _currentPrimary;
  bool get isDark => _isDark;
}

final dynamicThemeProvider =
    StateNotifierProvider<DynamicThemeNotifier, ThemeData>((ref) {
  final notifier = DynamicThemeNotifier(ref);
  ref.listen(authProvider, (previous, next) {
    notifier.loadTheme();
  });
  return notifier;
});
