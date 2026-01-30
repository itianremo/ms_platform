import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  runApp(const ProviderScope(child: WisslerApp()));
}

class WisslerApp extends StatelessWidget {
  const WisslerApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Wissler',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.pink),
        useMaterial3: true,
      ),
      home: const Scaffold(
        body: Center(child: Text('Wissler - Matching App')),
      ),
    );
  }
}
