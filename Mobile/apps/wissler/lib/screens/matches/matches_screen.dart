import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import '../../widgets/wissler_header.dart';
import '../home/wissler_home_screen.dart'; // Import Global Key
import '../matches/chat_detail_screen.dart';

// Wrapper for Nested Navigation
class MatchesScreen extends StatelessWidget {
  const MatchesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Navigator(
      onGenerateRoute: (settings) {
        return MaterialPageRoute(
          builder: (context) => const MatchesListScreen(),
        );
      },
    );
  }
}

class MatchesListScreen extends ConsumerWidget {
  const MatchesListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      appBar: const WisslerHeader(title: "Matches & Chat"),
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Who Liked Me Header (Link)
          Padding(
              padding: const EdgeInsets.only(left: 16.0, top: 10, bottom: 5),
              child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text("Who Likes Me",
                        style: TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 18,
                            color: Colors.blueGrey)),
                    TextButton(
                        onPressed: () {
                          // Switch to Likes Tab (Index 1)
                          wisslerHomeKey.currentState?.setIndex(1);
                        },
                        child: const Text("See All"))
                  ])),
          SizedBox(
              height: 100,
              child: FutureBuilder<List<Recommendation>>(
                  future: ref
                      .read(authClientProvider)
                      .getWhoLikesMe('22222222-2222-2222-2222-222222222222'),
                  builder: (context, snapshot) {
                    if (!snapshot.hasData)
                      return const Center(child: CircularProgressIndicator());
                    final likes = snapshot.data!;
                    return ListView.builder(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 10),
                        itemCount: likes.length + 1,
                        itemBuilder: (context, index) {
                          if (index == 0) {
                            return Padding(
                                padding: const EdgeInsets.only(right: 15),
                                child: Column(children: [
                                  GestureDetector(
                                      onTap: () {
                                        // Switch to Likes Tab
                                        wisslerHomeKey.currentState
                                            ?.setIndex(1);
                                      },
                                      child: const CircleAvatar(
                                          radius: 30,
                                          backgroundColor: Colors.amber,
                                          child: Icon(Icons.favorite,
                                              color: Colors.white))),
                                  const SizedBox(height: 5),
                                  const Text("See Likes",
                                      style: TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 12))
                                ]));
                          }
                          final user = likes[index - 1];
                          return Padding(
                              padding: const EdgeInsets.only(right: 15),
                              child: Column(children: [
                                ImageFiltered(
                                    imageFilter:
                                        ImageFilter.blur(sigmaX: 4, sigmaY: 4),
                                    child: CircleAvatar(
                                        radius: 30,
                                        backgroundImage:
                                            NetworkImage(user.avatarUrl))),
                                const SizedBox(height: 5),
                                Text(user.displayName,
                                    style: const TextStyle(
                                        fontWeight: FontWeight.bold))
                              ]));
                        });
                  })),
          const Divider(),
          const Padding(
            padding: EdgeInsets.all(16.0),
            child: Text("New Matches",
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ),
          SizedBox(
              height: 80,
              child: ListView.builder(
                  scrollDirection: Axis.horizontal,
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: 5,
                  itemBuilder: (context, index) {
                    return GestureDetector(
                      onTap: () {
                        Navigator.push(
                            context,
                            MaterialPageRoute(
                                builder: (_) => ChatDetailScreen(
                                    userId: "match_$index", // Mock ID
                                    userName: "Match $index",
                                    avatarUrl:
                                        'https://i.pravatar.cc/150?u=${index + 200}')));
                      },
                      child: Padding(
                          padding: const EdgeInsets.only(right: 15),
                          child: Column(children: [
                            CircleAvatar(
                                radius: 25,
                                backgroundImage: NetworkImage(
                                    'https://i.pravatar.cc/150?u=${index + 200}')),
                            const SizedBox(height: 4),
                            Text("Match $index",
                                style: const TextStyle(fontSize: 10))
                          ])),
                    );
                  })),
          const Padding(
            padding: EdgeInsets.all(16.0),
            child: Text("Messages",
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ),
          Expanded(
              child: ListView.separated(
                  itemCount: 5,
                  separatorBuilder: (_, __) => const Divider(height: 1),
                  itemBuilder: (context, index) {
                    return ListTile(
                        leading: CircleAvatar(
                            backgroundImage: NetworkImage(
                                'https://i.pravatar.cc/150?u=${index + 100}')),
                        title: Text("User $index"),
                        subtitle: const Text("Hey! How are you doing?"),
                        trailing: const Text("10:00 AM",
                            style: TextStyle(color: Colors.grey, fontSize: 12)),
                        onTap: () {
                          Navigator.push(
                              context,
                              MaterialPageRoute(
                                  builder: (_) => ChatDetailScreen(
                                      userId: "user_$index", // Mock ID
                                      userName: "User $index",
                                      avatarUrl:
                                          'https://i.pravatar.cc/150?u=${index + 100}')));
                        });
                  }))
        ],
      ),
    );
  }
}
