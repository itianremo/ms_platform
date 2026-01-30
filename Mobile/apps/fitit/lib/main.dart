import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  runApp(const ProviderScope(child: FitItApp()));
}

class FitItApp extends StatelessWidget {
  const FitItApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'FitIt',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.blue),
        useMaterial3: true,
      ),
      home: const Scaffold(
        body: Center(child: Text('FitIt - Workouts App')),
      ),
    );
  }
}
