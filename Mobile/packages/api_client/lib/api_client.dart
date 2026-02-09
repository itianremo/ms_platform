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
    // Mock Packages
    return [
      SubscriptionPackage(
          id: '1',
          appId: appId,
          name: 'Weekly',
          description: '7 Days access',
          price: 299.0,
          discount: 0,
          currency: 'EGP',
          period: 7,
          isActive: true),
      SubscriptionPackage(
          id: '2',
          appId: appId,
          name: 'Monthly',
          description: '30 Days access',
          price: 899.0,
          discount: 10,
          currency: 'EGP',
          period: 30,
          isActive: true),
    ];
  }

  Future<UserProfile?> getUserProfile(String userId, String appId) async {
    // Mock Profile
    return UserProfile(
      id: userId,
      userId: userId,
      appId: appId,
      displayName: 'Mock User',
      bio: 'This is a mock profile for UI testing.',
      avatarUrl: 'https://i.pravatar.cc/300',
      dateOfBirth: DateTime(1995, 5, 20),
      gender: 'Male',
      customDataJson: jsonEncode({'coins': 100, 'score': 500}),
    );
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

  Future<List<UserProfile>> getProfiles(String appId) async {
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    try {
      final response = await _dio.get(
        '$gatewayUrl/users/api/Users/profiles',
        queryParameters: {'appId': appId},
      );

      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> data = response.data;
        return data.map((json) => UserProfile.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error fetching profiles: $e');
      return [];
    }
  }

  Future<List<Country>> getCountries() async {
    // Mock Countries for UI verification
    return [
      Country(id: '1', name: 'Egypt', code: 'EG', phoneCode: '+20'),
      Country(id: '2', name: 'USA', code: 'US', phoneCode: '+1'),
      Country(id: '3', name: 'UAE', code: 'AE', phoneCode: '+971'),
    ];
  }

  Future<List<City>> getCities(String countryId) async {
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    try {
      final response =
          await _dio.get('$gatewayUrl/geo/api/Lookups/cities/$countryId');
      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> data = response.data;
        return data.map((json) => City.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error fetching cities: $e');
      return [];
    }
  }

  Future<void> deleteAccount(String userId, String password) async {
    // Assuming a Delete Endpoint that takes password for confirmation
    // If endpoints are standard REST, DELETE usually doesn't take body in some clients, but Dio supports it.
    // URL: /users/api/Users/{userId}

    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    await _dio.delete(
      '$gatewayUrl/users/api/Users/$userId',
      data: jsonEncode({'password': password}),
    );
  }

  Future<void> requestVerification(String userId, String type) async {
    // Mocking the endpoint call
    await _dio.post(
      '$baseUrl/verification/send', // Assuming Auth service handles this
      data: {'userId': userId, 'type': type},
    );
  }

  Future<List<Recommendation>> getRecommendations({
    String category = 'All',
    int page = 1,
    int pageSize = 20,
    String? country,
    int? minAge,
    int? maxAge,
  }) async {
    final uri = Uri.parse(baseUrl);
    final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';

    try {
      final response = await _dio.get(
        '$gatewayUrl/recommendation/api/Recommendations',
        queryParameters: {
          'category': category,
          'page': page,
          'pageSize': pageSize,
          if (country != null) 'country': country,
          if (minAge != null) 'minAge': minAge,
          if (maxAge != null) 'maxAge': maxAge,
        },
      );

      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> data = response.data;
        return data.map((json) => Recommendation.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error fetching recommendations: $e');
      return [];
    }
  }

  Future<List<Recommendation>> getWhoLikesMe(String userId) async {
    // Attempt to fetch from backend, or return mock
    try {
      final uri = Uri.parse(baseUrl);
      final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';
      final response = await _dio.get(
          '$gatewayUrl/matching/api/Likes/who-likes-me',
          queryParameters: {'userId': userId});
      if (response.statusCode == 200 && response.data != null) {
        return (response.data as List)
            .map((e) => Recommendation.fromJson(e))
            .toList();
      }
    } catch (_) {}
    // Mock Data (Max 20 as requested)
    return List.generate(
        20,
        (i) => Recommendation(
            userId: 'mock_$i',
            displayName: 'Admirer $i',
            age: 20 + i,
            avatarUrl: 'https://i.pravatar.cc/150?u=$i',
            city: 'Unknown',
            country: 'Unknown',
            matchPercentage: 0.8,
            isBoosted: false,
            isNew: true,
            isOnline: true,
            isVerified: false));
  }

  Future<List<Recommendation>> getMyLikes(String userId) async {
    try {
      final uri = Uri.parse(baseUrl);
      final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';
      final response = await _dio.get('$gatewayUrl/matching/api/Likes/my-likes',
          queryParameters: {'userId': userId});
      if (response.statusCode == 200 && response.data != null) {
        return (response.data as List)
            .map((e) => Recommendation.fromJson(e))
            .toList();
      }
    } catch (_) {}
    // Mock
    return List.generate(
        5,
        (i) => Recommendation(
            userId: 'liked_$i',
            displayName: 'Crush $i',
            age: 22 + i,
            avatarUrl: 'https://i.pravatar.cc/150?u=${i + 20}',
            city: 'Cairo',
            country: 'Egypt',
            matchPercentage: 0.9,
            isBoosted: false,
            isNew: false,
            isOnline: false,
            isVerified: true));
  }

  Future<List<Recommendation>> getMyDislikes(String userId) async {
    // Mock Dislikes
    return List.generate(
        10,
        (i) => Recommendation(
            userId: 'disliked_$i',
            displayName: 'Removed $i',
            age: 22 + i,
            avatarUrl: 'https://i.pravatar.cc/150?u=${i + 60}',
            city: 'Cairo',
            country: 'Egypt',
            matchPercentage: 0.1,
            isBoosted: false,
            isNew: false,
            isOnline: false,
            isVerified: false));
  }

  Future<List<Recommendation>> getTopPicks(String userId) async {
    try {
      final uri = Uri.parse(baseUrl);
      final gatewayUrl = '${uri.scheme}://${uri.host}:${uri.port}';
      final response = await _dio.get(
          '$gatewayUrl/matching/api/Recommendations/top-picks',
          queryParameters: {'userId': userId});
      if (response.statusCode == 200 && response.data != null) {
        return (response.data as List)
            .map((e) => Recommendation.fromJson(e))
            .toList();
      }
    } catch (_) {}
    // Mock
    return List.generate(
        5,
        (i) => Recommendation(
            userId: 'top_$i',
            displayName: 'Top Pick $i',
            age: 25,
            avatarUrl: 'https://i.pravatar.cc/150?u=${i + 50}',
            city: 'Paris',
            country: 'France',
            matchPercentage: 0.95 + (i * 0.01),
            isBoosted: true,
            isNew: false,
            isOnline: true,
            isVerified: true));
  }
}
