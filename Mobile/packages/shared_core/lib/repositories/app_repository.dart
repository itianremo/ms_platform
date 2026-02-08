import 'package:api_client/api_client.dart';

class AppRepository {
  final AuthClient _client;

  AppRepository(this._client);

  Future<AppConfig> getAppConfig(String appId) async {
    return await _client.getAppConfig(appId);
  }

  Future<UserProfile?> getUserProfile(String userId, String appId) async {
    return await _client.getUserProfile(userId, appId);
  }
}
