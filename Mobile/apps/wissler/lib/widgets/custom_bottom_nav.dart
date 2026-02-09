import 'package:flutter/material.dart';
import 'menu_background_painter.dart';
import 'wissler_header.dart';

class CustomBottomNavBar extends StatelessWidget {
  final int currentIndex;
  final Function(int) onTabSelected;

  const CustomBottomNavBar(
      {super.key, required this.currentIndex, required this.onTabSelected});

  @override
  Widget build(BuildContext context) {
    final primary = Theme.of(context).primaryColor;
    final Size size = MediaQuery.of(context).size;
    final double safeAreaBottom = MediaQuery.of(context).padding.bottom;
    final double totalHeight = 85 + safeAreaBottom;

    return SizedBox(
      width: size.width,
      height: totalHeight, // Slightly taller + Safe Area
      child: Stack(
        alignment: Alignment.bottomCenter,
        children: [
          // Background Shape
          CustomPaint(
            size: Size(size.width, totalHeight),
            painter: MenuBackgroundPainter(),
          ),

          // Icons Row
          Positioned(
            top: 20,
            left: 0,
            right: 0,
            child: SizedBox(
              height: 65,
              child: Row(
                mainAxisAlignment:
                    MainAxisAlignment.center, // Expanded handles distribution
                children: [
                  Expanded(
                      child:
                          _buildNavItem(Icons.radar, "Discover", 0, primary)),
                  Expanded(
                      child: _buildNavItem(
                          Icons.favorite_border, "Likes", 1, primary)),

                  // Center Space for Logo (Logo is 60, so 70 gives 5px padding)
                  const SizedBox(width: 70),

                  Expanded(
                      child: _buildNavItem(
                          Icons.chat_bubble_outline, "Chat", 3, primary)),
                  Expanded(
                      child: _buildNavItem(Icons.more_horiz, "More", 4, primary,
                          isMore: true)),
                ],
              ),
            ),
          ),

          // Floating Center Logo
          Positioned(
            bottom: 10 + safeAreaBottom,
            child: GestureDetector(
              onTap: () => onTabSelected(2),
              child: Container(
                padding: const EdgeInsets.all(4), // Space for border
                decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: Colors
                        .white, // Ensure it covers the bar line if overlaps
                    border: Border.all(
                        color: Colors.orange, width: 2), // 2px Orange Border
                    boxShadow: [
                      BoxShadow(
                          color: Colors.black.withOpacity(0.1), blurRadius: 5)
                    ]),
                child: const WisslerLogo(size: 58), // Slightly larger
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildNavItem(IconData icon, String label, int index, Color primary,
      {bool isMore = false}) {
    final isSelected = currentIndex == index;
    final color = isSelected ? primary : Colors.grey;

    return GestureDetector(
      onTap: () => onTabSelected(index),
      behavior: HitTestBehavior.opaque,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, color: color, size: 26),
          const SizedBox(height: 3),
          Text(
            label,
            style: TextStyle(
              color: color,
              fontSize: 11,
              fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
            ),
          ),
        ],
      ),
    );
  }
}
