import 'package:api_client/api_client.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AuthRepository {
  final AuthClient _client;
  final SharedPreferences _prefs;

  AuthRepository(this._client, this._prefs);

  Future<AuthResponse> login(LoginRequest request) async {
    final response = await _client.login(request);
    await _saveTokens(response);
    return response;
  }

  Future<String> register(RegisterRequest request) async {
    return await _client.register(request);
  }

  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    return await _client.forgotPassword(request);
  }

  Future<AuthResponse?> refreshToken() async {
    try {
      final response = await _client.refreshToken();
      await _saveTokens(response);
      return response;
    } catch (e) {
      return null;
    }
  }

  Future<void> logout() async {
    await _prefs.remove('access_token');
    await _prefs.remove('refresh_token');
  }

  Future<void> _saveTokens(AuthResponse response) async {
    await _prefs.setString('access_token', response.accessToken);
    await _prefs.setString('refresh_token', response.refreshToken);
  }

  String? getAccessToken() {
    return _prefs.getString('access_token');
  }
}
