import 'dart:async';
import 'package:flutter/material.dart';
import 'package:api_client/api_client.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';

class VerificationScreen extends ConsumerStatefulWidget {
  final String userId;
  final String type; // 'email' or 'sms'

  const VerificationScreen(
      {super.key, required this.userId, required this.type});

  @override
  ConsumerState<VerificationScreen> createState() => _VerificationScreenState();
}

class _VerificationScreenState extends ConsumerState<VerificationScreen> {
  final _otpController = TextEditingController();
  int _timerSeconds = 60;
  Timer? _timer;
  bool _canResend = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _startTimer();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _otpController.dispose();
    super.dispose();
  }

  void _startTimer() {
    setState(() {
      _timerSeconds = 60;
      _canResend = false;
    });
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_timerSeconds > 0) {
        setState(() => _timerSeconds--);
      } else {
        _timer?.cancel();
        setState(() => _canResend = true);
      }
    });
  }

  Future<void> _resendOtp() async {
    setState(() => _isLoading = true);
    try {
      final client = ref.read(authClientProvider);
      await client.requestVerification(widget.userId, widget.type);
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(const SnackBar(content: Text("OTP Resent!")));
        _startTimer();
      }
    } catch (e) {
      if (mounted)
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text("Error: $e")));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _verify() async {
    if (_otpController.text.length < 4) return;
    // Mock verification logic for now as endpoint wasn't specified in detail
    Navigator.pop(context, true); // Return true on success
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
          title: Text("Verify ${widget.type == 'email' ? 'Email' : 'Mobile'}")),
      body: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          children: [
            Text("Enter the OTP sent to your ${widget.type}",
                style: Theme.of(context).textTheme.bodyLarge),
            const SizedBox(height: 20),
            TextField(
              controller: _otpController,
              keyboardType: TextInputType.number,
              textAlign: TextAlign.center,
              style: const TextStyle(letterSpacing: 10, fontSize: 24),
              decoration: const InputDecoration(
                hintText: "0000",
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 20),
            if (_isLoading)
              const CircularProgressIndicator()
            else if (_canResend)
              TextButton(
                  onPressed: _resendOtp, child: const Text("Resend Code"))
            else
              Text("Resend code in $_timerSeconds s"),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                  onPressed: _verify, child: const Text("Verify")),
            )
          ],
        ),
      ),
    );
  }
}
