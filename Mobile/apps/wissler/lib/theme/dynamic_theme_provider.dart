import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/app_provider.dart';
import 'package:shared_core/providers/auth_provider.dart';
import '../config/constants.dart';
import 'wissler_theme.dart';

class DynamicThemeNotifier extends StateNotifier<ThemeData> {
  final Ref _ref;

  DynamicThemeNotifier(this._ref) : super(WisslerTheme.theme) {
    loadTheme();
  }

  Future<void> loadTheme() async {
    final authState = _ref.read(authProvider);

    try {
      if (authState.status == AuthStatus.authenticated) {
        await _loadAppConfigDefaults();
      } else {
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
        state = WisslerTheme.darkTheme;
      } else {
        state = WisslerTheme.theme;
      }
    } catch (e) {
      print('Error parsing theme json: $e');
    }
  }
}

final dynamicThemeProvider =
    StateNotifierProvider<DynamicThemeNotifier, ThemeData>((ref) {
  final notifier = DynamicThemeNotifier(ref);
  ref.listen(authProvider, (previous, next) {
    notifier.loadTheme();
  });
  return notifier;
});
