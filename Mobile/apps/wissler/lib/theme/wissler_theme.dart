import 'package:flutter/material.dart';

class WisslerTheme {
  // Light Theme - Orange Primary
  static ThemeData get theme {
    const primary = Color(0xFFF97316); // Orange 500
    const background = Colors.white;
    const surface = Colors.white;
    const onSurface = Color(0xFF1E293B); // Slate 800

    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primary,
        primary: primary,
        background: background,
        surface: surface,
        onSurface: onSurface,
      ),
      scaffoldBackgroundColor: background,

      // Add other theme details if needed to match Admin
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.white,
        foregroundColor: Color(0xFF1E293B),
        elevation: 0,
      ),

      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
        ),
      ),
    );
  }

  // Dark Theme - Blue Primary (Same as FitIt/Admin Dark)
  static ThemeData get darkTheme {
    const primary = Color(0xFF3B82F6); // Blue 500
    const background = Color(0xFF020617); // Slate 950
    const surface = Color(0xFF020617); // Slate 950
    const onSurface = Color(0xFFF8FAFC); // Slate 50
    const secondary = Color(0xFF1E293B); // Slate 800
    const muted = Color(0xFF1E293B); // Slate 800
    const mutedForeground = Color(0xFF94A3B8); // Slate 400
    const destructive = Color(0xFFEF4444); // Red 500

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      scaffoldBackgroundColor: background,
      colorScheme: const ColorScheme.dark(
        primary: primary,
        secondary: secondary,
        surface: surface,
        onSurface: onSurface,
        error: destructive,
      ),

      // Typography
      textTheme: const TextTheme(
        displayMedium: TextStyle(
          fontSize: 28,
          fontWeight: FontWeight.w700,
          color: onSurface,
          letterSpacing: -0.5,
        ),
        titleLarge: TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.w600,
          color: onSurface,
        ),
        bodyMedium: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.w500,
          color: onSurface,
        ),
        bodySmall: TextStyle(
          fontSize: 13,
          color: mutedForeground,
        ),
      ),

      // Buttons
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
        ),
      ),

      // Floating Action Button
      floatingActionButtonTheme: FloatingActionButtonThemeData(
        backgroundColor: primary,
        foregroundColor: Colors.white,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
      ),

      // Inputs
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: secondary,
        contentPadding: const EdgeInsets.all(16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: muted),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: muted),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: primary, width: 2),
        ),
        hintStyle: const TextStyle(color: mutedForeground),
      ),
    );
  }
}
