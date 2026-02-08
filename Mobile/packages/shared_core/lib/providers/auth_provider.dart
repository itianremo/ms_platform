import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:dio/dio.dart';
import 'package:api_client/api_client.dart';
import '../repositories/auth_repository.dart';

// Dio Provider - should be configured with BaseOptions, Interceptors, etc. somewhere globally.
final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(BaseOptions(
    baseUrl: 'http://192.168.100.16:7032/auth/api/Auth', // Gateway URL
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
  ));

  // Add interceptor to inject token?
  return dio;
});

final sharedPreferencesProvider = Provider<SharedPreferences>((ref) {
  throw UnimplementedError('Initialize this in main.dart via overrides');
});

final authClientProvider = Provider<AuthClient>((ref) {
  final dio = ref.watch(dioProvider);
  return AuthClient(dio, baseUrl: dio.options.baseUrl);
});

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepository(
    ref.watch(authClientProvider),
    ref.watch(sharedPreferencesProvider),
  );
});

enum AuthStatus { unknown, authenticated, unauthenticated }

class AuthState {
  final AuthStatus status;
  final bool isLoading;
  final String? error;

  AuthState(
      {this.status = AuthStatus.unknown, this.isLoading = false, this.error});
}

class AuthNotifier extends StateNotifier<AuthState> {
  final AuthRepository _repository;

  AuthNotifier(this._repository) : super(AuthState()) {
    _checkStatus();
  }

  Future<void> _checkStatus() async {
    final token = _repository.getAccessToken();
    if (token != null) {
      state = AuthState(status: AuthStatus.authenticated);
    } else {
      state = AuthState(status: AuthStatus.unauthenticated);
    }
  }

  Future<void> login(String email, String password, {String? appId}) async {
    state = AuthState(status: state.status, isLoading: true);
    try {
      await _repository
          .login(LoginRequest(email: email, password: password, appId: appId));
      state = AuthState(status: AuthStatus.authenticated, isLoading: false);
    } catch (e) {
      state = AuthState(
          status: AuthStatus.unauthenticated,
          isLoading: false,
          error: e.toString());
    }
  }

  Future<void> register(
      String email, String password, String firstName, String lastName,
      {String? phone}) async {
    state = AuthState(status: state.status, isLoading: true);
    try {
      await _repository.register(RegisterRequest(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        phone: phone,
      ));
      // Auto login? or require login?
      // For now, let's assume valid registration -> user needs to login or verify.
      // If login is required after register:
      state = AuthState(status: AuthStatus.unauthenticated, isLoading: false);
      // Maybe show success message?
    } catch (e) {
      state = AuthState(
          status: state.status, isLoading: false, error: e.toString());
    }
  }

  Future<void> forgotPassword(String email, {String? appId}) async {
    state = AuthState(status: state.status, isLoading: true);
    try {
      await _repository
          .forgotPassword(ForgotPasswordRequest(email: email, appId: appId));
      state = AuthState(status: state.status, isLoading: false);
    } catch (e) {
      state = AuthState(
          status: state.status, isLoading: false, error: e.toString());
    }
  }

  Future<void> logout() async {
    await _repository.logout();
    state = AuthState(status: AuthStatus.unauthenticated);
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref.watch(authRepositoryProvider));
});
