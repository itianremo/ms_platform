import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../theme/fitit_theme.dart';
import 'package:shared_core/providers/auth_provider.dart';

class FitItHomeScreen extends ConsumerWidget {
  const FitItHomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Access auth provider to logout
    final authNotifier = ref.read(authProvider.notifier);

    // Get real user name or fallback (Token decoding not yet implemented)
    const userName = 'Runner';

    return Scaffold(
      backgroundColor: FitItTheme.backgroundLight,
      appBar: AppBar(
        title: const Text('FitIt Dashboard'),
        centerTitle: true,
        backgroundColor: Colors.white,
        elevation: 0,
        iconTheme: const IconThemeData(color: FitItTheme.textDark),
        titleTextStyle: const TextStyle(
          color: FitItTheme.textDark,
          fontWeight: FontWeight.bold,
          fontSize: 18,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications_none_rounded),
            onPressed: () {},
          ),
          IconButton(
            icon: const Icon(Icons.logout, color: FitItTheme.adminBlue),
            onPressed: () => authNotifier.logout(),
          )
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Welcome Section with Real Data
            Text('Hello, $userName',
                style: Theme.of(context).textTheme.displayMedium),
            Text('Here is your daily activity.',
                style: Theme.of(context).textTheme.bodySmall),
            const SizedBox(height: 24),

            // Main Stats Grid - Structuring for future API integration
            // TODO: Replace with real API call to Health Service when available
            Row(
              children: [
                Expanded(
                    child: _buildStatCard(context, 'STEPS', '8,432', '/ 10k',
                        Icons.directions_walk, Colors.teal)),
                const SizedBox(width: 16),
                Expanded(
                    child: _buildStatCard(context, 'CALORIES', '640', 'kcal',
                        Icons.local_fire_department, Colors.orange)),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                    child: _buildStatCard(context, 'HEART PT', '45', 'pts',
                        Icons.favorite, Colors.pink)),
                const SizedBox(width: 16),
                Expanded(
                    child: _buildStatCard(context, 'SLEEP', '7h 12m', '',
                        Icons.bedtime, Colors.indigo)),
              ],
            ),

            const SizedBox(height: 32),

            // "Admin Style" Section Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Recent Workouts',
                    style: Theme.of(context).textTheme.titleLarge),
                TextButton(onPressed: () {}, child: const Text('View All')),
              ],
            ),
            const SizedBox(height: 8),

            // List of Workouts (Card style)
            _buildWorkoutItem(context, 'Running', '5.2 km', '32:10'),
            _buildWorkoutItem(context, 'Cycling', '12.4 km', '45:00'),
            _buildWorkoutItem(context, 'Swimming', '1.5 km', '30:00'),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {},
        child: const Icon(Icons.add),
      ),
      bottomNavigationBar: NavigationBar(
        backgroundColor: Colors.white,
        elevation: 1, // Subtle border/shadow
        indicatorColor: FitItTheme.adminBlue.withOpacity(0.1),
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.dashboard_outlined),
              selectedIcon: Icon(Icons.dashboard),
              label: 'Home'),
          NavigationDestination(
              icon: Icon(Icons.bar_chart_outlined),
              selectedIcon: Icon(Icons.bar_chart),
              label: 'Analytics'),
          NavigationDestination(
              icon: Icon(Icons.person_outline),
              selectedIcon: Icon(Icons.person),
              label: 'Profile'),
        ],
      ),
    );
  }

  Widget _buildStatCard(BuildContext context, String title, String value,
      String unit, IconData icon, Color color) {
    return Card(
      color: Colors.white,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(color: Colors.grey.shade200, width: 1),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: color.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Icon(icon, color: color, size: 20),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Text(value,
                style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                    color: FitItTheme.textDark)),
            Text('$title $unit',
                style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: Colors.grey)),
          ],
        ),
      ),
    );
  }

  Widget _buildWorkoutItem(
      BuildContext context, String title, String distance, String time) {
    return Card(
      color: Colors.white,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(color: Colors.grey.shade200, width: 1),
      ),
      margin: const EdgeInsets.only(bottom: 12),
      child: ListTile(
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: FitItTheme.adminBlue.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: const Icon(Icons.fitness_center, color: FitItTheme.adminBlue),
        ),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text('$distance • $time'),
        trailing: const Icon(Icons.chevron_right, color: Colors.grey),
      ),
    );
  }
}

class GraphPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = FitItTheme.adminBlue
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    final path = Path();
    path.moveTo(0, size.height * 0.8);
    path.lineTo(size.width * 0.2, size.height * 0.6);
    path.lineTo(size.width * 0.4, size.height * 0.7);
    path.lineTo(size.width * 0.6, size.height * 0.3);
    path.lineTo(size.width * 0.8, size.height * 0.4);
    path.lineTo(size.width, size.height * 0.1);

    canvas.drawPath(path, paint);

    // Fill
    final fillPaint = Paint()
      ..color = FitItTheme.adminBlue.withOpacity(0.1)
      ..style = PaintingStyle.fill;

    path.lineTo(size.width, size.height);
    path.lineTo(0, size.height);
    path.close();
    canvas.drawPath(path, fillPaint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
