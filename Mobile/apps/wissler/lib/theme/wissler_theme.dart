import 'package:flutter/material.dart';

class WisslerPalette {
  static const coral = Color(0xFFFF6B6B);
  static const teal = Color(0xFF11998E);
  static const purple = Color(0xFF9D4EDD);
  static const orange = Color(0xFFF97316); // Updated to Wissler Orange
  static const pink = Color(0xFFFF0080);
}

class WisslerTheme {
  static ThemeData getTheme({
    required Color primary,
    required bool isDark,
  }) {
    final background = isDark ? const Color(0xFF020617) : Colors.white;
    final surface = isDark ? const Color(0xFF020617) : Colors.white;
    final onSurface =
        isDark ? const Color(0xFFF8FAFC) : const Color(0xFF1E293B);
    final secondary =
        isDark ? const Color(0xFF1E293B) : const Color(0xFFF1F5F9);
    final muted = isDark ? const Color(0xFF334155) : const Color(0xFFE2E8F0);
    final mutedForeground =
        isDark ? const Color(0xFF94A3B8) : const Color(0xFF64748B);

    return ThemeData(
      useMaterial3: true,
      brightness: isDark ? Brightness.dark : Brightness.light,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primary,
        primary: primary,
        background: background,
        surface: surface,
        onSurface: onSurface,
        secondary: secondary,
        error: const Color(0xFFEF4444),
        brightness: isDark ? Brightness.dark : Brightness.light,
      ),
      scaffoldBackgroundColor: background,

      appBarTheme: AppBarTheme(
        backgroundColor: isDark ? background : Colors.white,
        foregroundColor: onSurface,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
      ),

      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
        ),
      ),

      floatingActionButtonTheme: FloatingActionButtonThemeData(
        backgroundColor: primary,
        foregroundColor: Colors.white,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
      ),

      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: secondary,
        contentPadding: const EdgeInsets.all(16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(color: muted),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(color: muted),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(color: primary, width: 2),
        ),
        hintStyle: TextStyle(color: mutedForeground),
      ),

      // Navigation Bar Specifics
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: isDark ? const Color(0xFF0F172A) : Colors.white,
        indicatorColor: primary.withOpacity(0.15),
        surfaceTintColor: Colors.transparent,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        iconTheme: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return IconThemeData(color: primary, size: 26);
          }
          return IconThemeData(color: mutedForeground, size: 24);
        }),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return TextStyle(
                color: primary, fontWeight: FontWeight.w600, fontSize: 12);
          }
          return TextStyle(
              color: mutedForeground,
              fontWeight: FontWeight.normal,
              fontSize: 12);
        }),
      ),

      // Card Theme
      // Card Theme
      // cardTheme: CardTheme(
      //   color: surface,
      //   surfaceTintColor: Colors.transparent,
      //   elevation: 2,
      //   shadowColor: Colors.black.withOpacity(0.05),
      //   shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      //   margin: const EdgeInsets.symmetric(vertical: 8, horizontal: 0),
      // ),
    );
  }
}
