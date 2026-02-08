import 'dart:convert';
import 'package:dio/dio.dart';
import 'models.dart';

export 'models.dart';

class AuthClient {
  final Dio _dio;
  final String baseUrl;

  AuthClient(this._dio,
      {this.baseUrl = "http://192.168.100.16:7032/auth/api/Auth"});

  Future<AuthResponse> login(LoginRequest request) async {
    final response = await _dio.post(
      '$baseUrl/login',
      data: request.toJson(),
    );
    return AuthResponse.fromJson(response.data);
  }

  Future<String> register(RegisterRequest request) async {
    final response = await _dio.post(
      '$baseUrl/register',
      data: request.toJson(),
    );
    // Assuming API returns { "userId": "..." } or similar
    // Check AuthController: return Ok(new { userId = result });
    return response.data['userId'].toString();
  }

  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    await _dio.post(
      '$baseUrl/password/forgot',
      data: request.toJson(),
    );
  }

  Future<AuthResponse> refreshToken() async {
    final response = await _dio.post('$baseUrl/refresh');
    return AuthResponse.fromJson(response.data);
  }

  // --- New Methods for Dynamic Theme ---

  Future<AppConfig> getAppConfig(String appId) async {
    // Gateway routes /apps/api/Apps/{id} -> Apps Service
    // baseUrl usually points to Auth Service in current generic cleanup.
    // We need to handle routing.
    // If baseUrl is http://.../api/Auth, we need to strip /Auth and append /Apps/api/Apps
    // Or just assume the caller handles the full URL if we use a new Client?
    // Let's use a relative path trick or inject a new baseUrl.
    // For now, let's assume the Dio instance defaults to Gateway root or we construct the path carefully.
    // The current baseUrl in AuthClient is ".../api/Auth".
    // Let's use `_dio` but override base URL for these calls if possible, or just use absolute path if we knew the gateway URL.
    // Actually, looking at `auth_provider.dart`, the Dio baseUrl is ".../api/Auth".
    // This is problematic for accessing other services.
    // I should probably update `api_client` to be more generic or accept a Gateway URL.
    // However, to minimize changes, I will use `../../apps/api/Apps/$appId` relative path if Dio supports it,
    // OR just replace `/Auth` with `/Apps` in the path.

    // Let's try to assume the Gateway is at the root of the current baseUrl.
    // If baseUrl is ".../api/Auth", then ".../apps/api/Apps" is a sibling.

    // Hacky but safe for this specific "Monorepo" style structure if valid:
    // Actually, better to add a separate method or Client for Apps/Users if we want to be clean.
    // But user asked to "apply this fix", so I will add it here.

    // We'll use a calculated path assuming standard Gateway routing.
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    final response = await _dio.get('$gatewayUrl/apps/api/Apps/$appId');
    return AppConfig.fromJson(response.data);
  }

  Future<List<SubscriptionPackage>> getSubscriptionPackages(
      String appId) async {
    try {
      final uri = Uri.parse(baseUrl);
      final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

      final response =
          await _dio.get('$gatewayUrl/apps/api/Apps/$appId/packages');
      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> data = response.data;
        return data.map((json) => SubscriptionPackage.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error fetching packages: $e');
      return [];
    }
  }

  Future<UserProfile?> getUserProfile(String userId, String appId) async {
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    final response = await _dio.get(
      '$gatewayUrl/users/api/Users/profile',
      queryParameters: {'userId': userId, 'appId': appId},
    );
    var data = response.data;
    if (data is String) {
      if (data.isEmpty) {
        return null;
      }
      try {
        data = jsonDecode(data);
      } catch (e) {
        throw FormatException('Failed to decode JSON: $e');
      }
    }
    if (data == null) {
      return null;
    }
    return UserProfile.fromJson(data);
  }

  Future<void> updateUserProfile(UserProfile profile) async {
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    await _dio.put(
      '$gatewayUrl/users/api/Users/profile',
      data: {
        'id': profile.id,
        'userId': profile.userId,
        'appId': profile.appId,
        'displayName': profile.displayName,
        'bio': profile.bio,
        'avatarUrl': profile.avatarUrl,
        'customDataJson': profile.customDataJson,
        'dateOfBirth': profile.dateOfBirth?.toIso8601String(),
        'gender': profile.gender,
      },
    );
  }
}
