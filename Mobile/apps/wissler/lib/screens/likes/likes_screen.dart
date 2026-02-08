import 'package:flutter/material.dart';

class LikesScreen extends StatelessWidget {
  const LikesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.favorite_rounded, size: 80, color: Colors.pinkAccent),
            SizedBox(height: 16),
            Text('People who liked you will appear here',
                style: TextStyle(color: Colors.grey)),
          ],
        ),
      ),
    );
  }
}
