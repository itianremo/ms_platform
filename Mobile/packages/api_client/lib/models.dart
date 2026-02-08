class LoginRequest {
  final String email;
  final String password;
  final String? appId;
  final String? ipAddress;
  final String? userAgent;

  LoginRequest({
    required this.email,
    required this.password,
    this.appId,
    this.ipAddress,
    this.userAgent,
  });

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
      if (appId != null) 'appId': appId,
      if (ipAddress != null) 'ipAddress': ipAddress,
      if (userAgent != null) 'userAgent': userAgent,
    };
  }
}

class RegisterRequest {
  final String email;
  final String? phone;
  final String password;
  final String? appId;
  final int verificationType; // 0 = Email, 1 = Phone
  final bool requiresAdminApproval;
  final String firstName;
  final String lastName;

  RegisterRequest({
    required this.email,
    this.phone,
    required this.password,
    this.appId,
    this.verificationType = 0,
    this.requiresAdminApproval = false,
    required this.firstName,
    required this.lastName,
  });

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'phone': phone,
      'password': password,
      if (appId != null) 'appId': appId,
      'verificationType': verificationType,
      'requiresAdminApproval': requiresAdminApproval,
      'firstName': firstName,
      'lastName': lastName,
    };
  }
}

class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final int expiresIn;

  AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresIn,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      expiresIn: json['expiresIn'] as int,
    );
  }
}

class ForgotPasswordRequest {
  final String email;
  final String? appId;

  ForgotPasswordRequest({
    required this.email,
    this.appId,
  });

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      if (appId != null) 'appId': appId,
    };
  }
}

class AppConfig {
  final String id;
  final String name;
  final String description;
  final String baseUrl;
  final bool isActive;
  final String? themeJson;
  final String? defaultUserProfileJson;

  AppConfig({
    required this.id,
    required this.name,
    required this.description,
    required this.baseUrl,
    required this.isActive,
    this.themeJson,
    this.defaultUserProfileJson,
  });

  factory AppConfig.fromJson(Map<String, dynamic> json) {
    return AppConfig(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String,
      baseUrl: json['baseUrl'] as String,
      isActive: json['isActive'] as bool,
      themeJson: json['themeJson'] as String?,
      defaultUserProfileJson: json['defaultUserProfileJson'] as String?,
    );
  }
}

class UserProfile {
  final String id;
  final String userId;
  final String appId;
  final String? displayName;
  final String? bio;
  final String? avatarUrl;
  final String? customDataJson;

  UserProfile({
    required this.id,
    required this.userId,
    required this.appId,
    this.displayName,
    this.bio,
    this.avatarUrl,
    this.customDataJson,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] as String,
      userId: json['userId'] as String,
      appId: json['appId'] as String,
      displayName: json['displayName'] as String?,
      bio: json['bio'] as String?,
      avatarUrl: json['avatarUrl'] as String?,
      customDataJson: json['customDataJson'] as String?,
    );
  }
}
