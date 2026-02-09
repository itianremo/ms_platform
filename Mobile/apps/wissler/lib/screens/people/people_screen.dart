import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../widgets/wissler_header.dart';
import '../home/swipe_screen.dart'; // For provider
import '../home/wissler_home_screen.dart'; // For key
import '../profile/user_profile_screen.dart';

class PeopleScreen extends ConsumerWidget {
  const PeopleScreen({super.key});

  static const List<Map<String, dynamic>> _categories = [
    {
      'name': 'Near You',
      'image':
          'https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?auto=format&fit=crop&w=600&q=80',
      'color': Colors.purple
    },
    // ... (rest of categories unchanged, assumed accessible or copied?)
    // Actually, need to include the list here since I'm mostly replacing the class structure.
    {
      'name': 'New',
      'image':
          'https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=600&q=80',
      'color': Colors.orange
    },
    {
      'name': 'Verified',
      'image':
          'https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?auto=format&fit=crop&w=600&q=80',
      'color': Colors.blue
    },
    {
      'name': 'Popular',
      'image':
          'https://images.unsplash.com/photo-1515378960530-7c0da6231fb1?auto=format&fit=crop&w=600&q=80',
      'color': Colors.red
    },
    {
      'name': 'Online',
      'image':
          'https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?auto=format&fit=crop&w=600&q=80',
      'color': Colors.green
    },
    {
      'name': 'My Type',
      'image':
          'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=600&q=80',
      'color': Colors.pink
    }
  ];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Create a mutable list from const categories
    List<Map<String, dynamic>> displayList = List.from(_categories);

    // ... (Promo logic unchanged)
    // Inject Promo Card randomly
    final promoCard = {
      'name': 'Wissler+',
      'image':
          'https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=600&q=80', // distinct business/premium look
      'color': const Color(0xFF003366), // Admin Blue
      'isPromo': true,
    };

    final random = DateTime.now().millisecondsSinceEpoch;
    int insertIndex = random % (displayList.length + 1);
    displayList.insert(insertIndex, promoCard);

    // Split categories into two columns for masonry effect
    final leftColumn = <Map<String, dynamic>>[];
    final rightColumn = <Map<String, dynamic>>[];

    for (int i = 0; i < displayList.length; i++) {
      if (i % 2 == 0)
        leftColumn.add(displayList[i]);
      else
        rightColumn.add(displayList[i]);
    }

    return Scaffold(
        appBar: const WisslerHeader(title: "Discover"),
        body: SingleChildScrollView(
            padding: const EdgeInsets.all(10),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(child: _buildColumn(context, ref, leftColumn, 0)),
                Expanded(
                    child: _buildColumn(context, ref, rightColumn,
                        1)), // Fixed SizedBox in row logic?
                // Original code had SizedBox between columns?
              ],
            )));
  }

  Widget _buildColumn(BuildContext context, WidgetRef ref,
      List<Map<String, dynamic>> items, int columnIndex) {
    return Column(
      children: items.map((cat) {
        final bool isPromo = cat['isPromo'] == true;

        // Random height
        final double randomHeight = 150.0 + (calendarHash(cat['name']) % 100);

        return GestureDetector(
            onTap: () {
              if (isPromo) {
                // Open Wissler Plus Modal
                showModalBottomSheet(
                    context: context,
                    isScrollControlled: true,
                    useRootNavigator: true,
                    useSafeArea: true,
                    backgroundColor: Colors.transparent,
                    builder: (context) => const ClipRRect(
                        borderRadius:
                            BorderRadius.vertical(top: Radius.circular(20)),
                        child: UserProfileScreen(isModal: true)));
              } else {
                // Determine Category
                final categoryName = cat['name'] as String;

                // Update Provider
                ref.read(swipeCategoryProvider.notifier).state = categoryName;

                // Switch Tab to Swipe (Index 2)
                wisslerHomeKey.currentState?.setIndex(2);

                // No Navigator.push!
              }
            },
            child: Container(
                margin: const EdgeInsets.only(bottom: 10),
                height: randomHeight,
                decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(15),
                    image: DecorationImage(
                        image: NetworkImage(cat['image']),
                        fit: BoxFit.cover,
                        colorFilter: ColorFilter.mode(
                            isPromo
                                ? Colors.black.withOpacity(0.6)
                                : Colors.black.withOpacity(0.3),
                            BlendMode.darken)),
                    border: isPromo
                        ? Border.all(
                            color: Colors.amber,
                            width: 3) // Thicker Gold Border for Promo
                        : Border.all(
                            color: Colors.amber.withOpacity(0.5), width: 1)),
                alignment: Alignment.center,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    if (isPromo)
                      Icon(Icons.star, color: Colors.amber, size: 40),
                    Text(isPromo ? "Get Wissler+" : cat['name'],
                        textAlign: TextAlign.center,
                        style: TextStyle(
                            color: Colors.white,
                            fontSize:
                                isPromo ? 28 : 24, // Bigger font for promo
                            fontWeight: FontWeight.bold,
                            letterSpacing: 1.2,
                            shadows: [
                              Shadow(
                                  blurRadius: 10,
                                  color: Colors.black,
                                  offset: Offset(0, 2))
                            ])),
                    if (isPromo)
                      Container(
                          margin: const EdgeInsets.only(top: 10),
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 5),
                          decoration: BoxDecoration(
                            color: Colors.amber,
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: const Text("UPGRADE",
                              style: TextStyle(
                                  color: Colors.black,
                                  fontWeight: FontWeight.bold,
                                  fontSize: 12)))
                  ],
                )));
      }).toList(),
    );
  }

  // Simple hash to get pseudo-random but stable height per session if needed,
  // but for "fresh" look we can mix in a timestamp if we want true random on refresh.
  int calendarHash(String input) {
    return input.codeUnits.fold(0, (a, b) => a + b) +
        DateTime.now().millisecond;
  }
}
