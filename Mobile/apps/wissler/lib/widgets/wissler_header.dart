import 'package:flutter/material.dart';

class WisslerHeader extends StatelessWidget implements PreferredSizeWidget {
  final String title;
  final List<Widget>? actions;
  const WisslerHeader({super.key, required this.title, this.actions});

  @override
  Widget build(BuildContext context) {
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.transparent,
      automaticallyImplyLeading: false,
      actions: actions,
      title: Row(
        children: [
          const WisslerLogo(size: 32),
          const Spacer(),
          Text(
            title,
            style: TextStyle(
              fontFamily: 'Cursive',
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: Theme.of(context).primaryColor,
            ),
          ),
        ],
      ),
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);
}

class WisslerLogo extends StatelessWidget {
  final double size;
  final Color? color;
  final Color? textColor;

  const WisslerLogo({super.key, this.size = 40, this.color, this.textColor});

  @override
  Widget build(BuildContext context) {
    // Default to Theme Primary if not provided
    final bgColor = color ?? Theme.of(context).primaryColor;
    return Container(
      width: size,
      height: size,
      alignment: Alignment.center,
      decoration:
          BoxDecoration(color: bgColor, shape: BoxShape.circle, boxShadow: [
        BoxShadow(
            color: bgColor.withOpacity(0.4),
            blurRadius: size / 4,
            offset: Offset(0, size / 8))
      ]),
      child: Text("W",
          style: TextStyle(
              color: textColor ?? Colors.white,
              fontWeight: FontWeight.bold,
              fontSize: size * 0.6 // Scale text with size
              )),
    );
  }
}
