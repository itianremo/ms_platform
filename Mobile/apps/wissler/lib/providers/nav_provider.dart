import 'package:flutter_riverpod/flutter_riverpod.dart';

// Tracks the current index of the bottom navigation bar
final currentNavIndexProvider =
    StateProvider<int>((ref) => 2); // Default to 2 (Home)
