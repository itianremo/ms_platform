import 'package:flutter/material.dart';

class FitItTheme {
  // Final Design: "DarkWC Light Mod"
  // Features: Light Background, No Glow, Admin Blue, Sharper Curved Edges

  static const Color adminBlue =
      Color(0xFF1565C0); // Blue 800 - Professional Admin Blue
  static const Color adminDarkBlue = Color(0xFF0D47A1); // Blue 900
  static const Color backgroundLight =
      Color(0xFFF5F5F7); // Apple-like light grey
  static const Color surfaceWhite = Colors.white;
  static const Color textDark = Color(0xFF2D3142); // Soft dark slate
  static const Color textGrey = Color(0xFF9094A6);

  static ThemeData get theme {
    return ThemeData(
      useMaterial3: true,
      scaffoldBackgroundColor: backgroundLight,
      colorScheme: const ColorScheme.light(
        primary: adminBlue,
        secondary: adminDarkBlue,
        surface: surfaceWhite,
        onSurface: textDark,
      ),

      // Card Theme: Sharper curves, no glow, subtle border
      // cardTheme: CardTheme(
      //   color: surfaceWhite,
      //   elevation: 0, // No glow/shadow
      //   shape: RoundedRectangleBorder(
      //     borderRadius: BorderRadius.circular(
      //         12), // "Little sharper" (standard is often 16-20)
      //     side: BorderSide(color: Colors.grey.shade200, width: 1),
      //   ),
      //   margin: const EdgeInsets.symmetric(vertical: 8, horizontal: 16),
      // ),

      // Typography: Clean, Professional
      textTheme: const TextTheme(
        displayMedium: TextStyle(
          fontSize: 28,
          fontWeight: FontWeight.w700,
          color: textDark,
          letterSpacing: -0.5,
        ),
        titleLarge: TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.w600,
          color: textDark,
        ),
        bodyMedium: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.w500,
          color: textDark,
        ),
        bodySmall: TextStyle(
          fontSize: 13,
          color: textGrey,
        ),
      ),

      // Buttons
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: adminBlue,
          foregroundColor: Colors.white,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8), // Sharper buttons
          ),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
        ),
      ),

      // Floating Action Button
      floatingActionButtonTheme: FloatingActionButtonThemeData(
        backgroundColor: adminBlue,
        foregroundColor: Colors.white,
        elevation: 0, // No glow
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12), // Sharper
        ),
      ),

      // Inputs
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surfaceWhite,
        contentPadding: const EdgeInsets.all(16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: Colors.grey.shade300),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: BorderSide(color: Colors.grey.shade300),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: adminBlue, width: 2),
        ),
      ),
    );
  }

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
          borderSide: const BorderSide(color: muted),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: muted),
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
