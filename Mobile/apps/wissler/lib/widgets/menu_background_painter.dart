import 'package:flutter/material.dart';

class MenuBackgroundPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    Paint paint = Paint()
      ..color = Colors.white
      ..style = PaintingStyle.fill;

    Path path = Path();
    path.moveTo(0, 20); // Start slightly lower for "floating" effect
    path.lineTo(size.width, 20); // Straight line across (Solid)

    path.lineTo(size.width, size.height);
    path.lineTo(0, size.height);
    path.close();
    path.lineTo(size.width, size.height);
    path.lineTo(0, size.height);
    path.close();

    canvas.drawShadow(path, Colors.black, 5, true);
    canvas.drawPath(path, paint);

    // 1px Top Border
    Paint borderPaint = Paint()
      ..color = Colors.orange
      ..strokeWidth =
          2.0 // Slightly thicker as requested (implied by "orange color of the theme")
      ..strokeWidth = 1.0
      ..style = PaintingStyle.stroke;

    canvas.drawLine(const Offset(0, 20), Offset(size.width, 20), borderPaint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) {
    return false;
  }
}
