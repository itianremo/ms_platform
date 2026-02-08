import 'package:flutter/material.dart';

class PeopleScreen extends StatelessWidget {
  const PeopleScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.people_rounded, size: 80, color: Colors.orangeAccent),
            SizedBox(height: 16),
            Text('Find people near you', style: TextStyle(color: Colors.grey)),
          ],
        ),
      ),
    );
  }
}
