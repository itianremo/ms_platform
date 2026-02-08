import 'package:flutter/material.dart';

class MatchingScreen extends StatelessWidget {
  const MatchingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.style, size: 80, color: Colors.grey),
            SizedBox(height: 16),
            Text('Matching Cards will appear here'),
          ],
        ),
      ),
    );
  }
}
