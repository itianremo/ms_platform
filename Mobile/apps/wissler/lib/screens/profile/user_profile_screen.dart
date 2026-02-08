import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:geolocator/geolocator.dart';
import 'package:geocoding/geocoding.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import '../../theme/dynamic_theme_provider.dart';

class UserProfileScreen extends ConsumerStatefulWidget {
  const UserProfileScreen({super.key});

  @override
  ConsumerState<UserProfileScreen> createState() => _UserProfileScreenState();
}

class _UserProfileScreenState extends ConsumerState<UserProfileScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  UserProfile? _userProfile;
  bool _isLoading = true;
  String? _errorMessage;

  // Profile Fields
  final _displayNameController = TextEditingController();
  final _bioController = TextEditingController();
  final _mobileController = TextEditingController();
  final _emailController = TextEditingController();
  final _cityController = TextEditingController();
  final _countryController = TextEditingController();

  // Settings
  bool _emailNotify = true;
  bool _smsNotify = true;
  bool _pushNotify = true;

  // Questions
  final _lookingForController = TextEditingController();
  final _interestsController = TextEditingController();

  // Image Picker
  final ImagePicker _picker = ImagePicker();
  XFile? _imageFile;

  List<SubscriptionPackage> _packages = [];
  bool _isLoadingPackages = false;
  String? _selectedPackageId;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadProfile();
    _loadPackages();
  }

  Future<void> _loadPackages() async {
    setState(() => _isLoadingPackages = true);
    try {
      final authClient = ref.read(authClientProvider);
      // Hardcoded Wissler AppID for now
      final pkgs = await authClient
          .getSubscriptionPackages('22222222-2222-2222-2222-222222222222');
      setState(() {
        _packages = pkgs.where((p) => p.isActive).toList();
        // Sort by price
        _packages.sort((a, b) => a.price.compareTo(b.price));
      });
    } catch (e) {
      print('Error loading packages: $e');
    } finally {
      if (mounted) setState(() => _isLoadingPackages = false);
    }
  }

  @override
  void dispose() {
    _tabController.dispose();
    _displayNameController.dispose();
    _bioController.dispose();
    _mobileController.dispose();
    _emailController.dispose();
    _cityController.dispose();
    _countryController.dispose();
    _lookingForController.dispose();
    _interestsController.dispose();
    super.dispose();
  }

  Future<void> _loadProfile() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final repo = ref.read(authRepositoryProvider);
      final token = repo.getAccessToken();

      if (token == null) throw Exception('User not logged in');

      final payload = _decodeJwt(token);
      final userId = payload['sub'] ?? payload['id'] ?? payload['userId'];

      if (userId == null) throw Exception('Invalid token');

      final authClient = ref.read(authClientProvider);
      final profile = await authClient.getUserProfile(
        userId,
        '22222222-2222-2222-2222-222222222222',
      );

      if (profile == null) {
        _userProfile = UserProfile(
          id: '',
          userId: userId,
          appId: '22222222-2222-2222-2222-222222222222',
          displayName: '',
          bio: '',
          avatarUrl: '',
          customDataJson: null,
          dateOfBirth: null,
          gender: null,
        );
      } else {
        _userProfile = profile;
        _parseCustomData(profile.customDataJson);
      }

      _displayNameController.text = _userProfile?.displayName ?? '';
      _bioController.text = _userProfile?.bio ?? '';

      // Populate Email from Token
      final email = payload['email'] ??
          payload[
              'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
          '';
      _emailController.text = email;

      // Populate Mobile from Token if not already set by customData
      if (_mobileController.text.isEmpty) {
        final phone = payload['phone'] ??
            payload[
                'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'] ??
            '';
        _mobileController.text = phone;
      }
    } catch (e) {
      _errorMessage = e.toString();
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _parseCustomData(String? jsonStr) {
    if (jsonStr == null) return;
    try {
      final data = json.decode(jsonStr);
      if (data is Map<String, dynamic>) {
        if (data.containsKey('mobile')) _mobileController.text = data['mobile'];
        if (data.containsKey('city')) _cityController.text = data['city'];
        if (data.containsKey('country'))
          _countryController.text = data['country'];
        if (data.containsKey('lookingFor'))
          _lookingForController.text = data['lookingFor'];
        if (data.containsKey('interests'))
          _interestsController.text = data['interests'];

        if (data.containsKey('emailNotify')) _emailNotify = data['emailNotify'];
        if (data.containsKey('smsNotify')) _smsNotify = data['smsNotify'];
        if (data.containsKey('pushNotify')) _pushNotify = data['pushNotify'];
      }
    } catch (e) {
      print('Error parsing custom data: $e');
    }
  }

  Future<void> _pickImage() async {
    try {
      final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
      if (image != null) {
        setState(() {
          _imageFile = image;
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error picking image: $e')),
      );
    }
  }

  Future<void> _getCurrentLocation() async {
    bool serviceEnabled;
    LocationPermission permission;

    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Location services are disabled.')));
      return;
    }

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) {
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Location permissions are denied')));
        return;
      }
    }

    if (permission == LocationPermission.deniedForever) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content: Text('Location permissions are permanently denied.')));
      return;
    }

    try {
      final position = await Geolocator.getCurrentPosition();
      List<Placemark> placemarks =
          await placemarkFromCoordinates(position.latitude, position.longitude);
      if (placemarks.isNotEmpty) {
        final place = placemarks.first;
        setState(() {
          _cityController.text = place.locality ?? '';
          _countryController.text = place.country ?? '';
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Error getting location: $e')));
    }
  }

  Future<void> _saveProfile() async {
    if (_userProfile == null) return;

    setState(() => _isLoading = true);

    try {
      final authClient = ref.read(authClientProvider);

      // Merge data into customData
      Map<String, dynamic> customData = {};
      if (_userProfile?.customDataJson != null) {
        try {
          customData = json.decode(_userProfile!.customDataJson!);
        } catch (_) {}
      }

      customData['mobile'] = _mobileController.text;
      customData['city'] = _cityController.text;
      customData['country'] = _countryController.text;
      customData['lookingFor'] = _lookingForController.text;
      customData['interests'] = _interestsController.text;

      customData['emailNotify'] = _emailNotify;
      customData['smsNotify'] = _smsNotify;
      customData['pushNotify'] = _pushNotify;

      // Handle Image Upload (Mock)
      String? finalAvatarUrl = _userProfile?.avatarUrl;
      if (_imageFile != null) {
        print("Simulating upload of ${_imageFile!.path}");
        // In real app, upload and update finalAvatarUrl
      }

      final updatedProfile = UserProfile(
        id: _userProfile!.id,
        userId: _userProfile!.userId,
        appId: _userProfile!.appId,
        displayName: _displayNameController.text,
        bio: _bioController.text,
        avatarUrl: finalAvatarUrl,
        customDataJson: json.encode(customData),
        dateOfBirth: _userProfile!.dateOfBirth,
        gender: _userProfile!.gender,
      );

      await authClient.updateUserProfile(updatedProfile);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Profile Saved Successfully')),
        );
        _userProfile = updatedProfile;
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error updating profile: $e')),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  // ... (Keep existing helpers like _decodeJwt, _decodeBase64, _getUserEmail, _toggleTheme, _changeColor, _saveThemePreferences)
  // Re-implementing them briefly for completeness in this file rewrite.

  Map<String, dynamic> _decodeJwt(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) throw Exception('Invalid token');
      final payload = _decodeBase64(parts[1]);
      final payloadMap = json.decode(payload);
      if (payloadMap is! Map<String, dynamic>) {
        throw Exception('Invalid payload');
      }
      return payloadMap;
    } catch (e) {
      return {};
    }
  }

  String _decodeBase64(String str) {
    String output = str.replaceAll('-', '+').replaceAll('_', '/');
    switch (output.length % 4) {
      case 0:
        break;
      case 2:
        output += '==';
        break;
      case 3:
        output += '=';
        break;
    }
    return utf8.decode(base64Url.decode(output));
  }

  Future<void> _toggleTheme(bool isDark) async {
    final notifier = ref.read(dynamicThemeProvider.notifier);
    notifier.toggleDarkMode(isDark);
    _saveThemePreferences(isDark, notifier.currentPrimary);
  }

  Future<void> _saveThemePreferences(bool isDark, Color primary) async {
    if (_userProfile == null) return;
    // Note: This duplicates logic with _saveProfile but is instant for UI responsiveness
    Map<String, dynamic> customData = {};
    if (_userProfile?.customDataJson != null) {
      try {
        customData = json.decode(_userProfile!.customDataJson!);
      } catch (_) {}
    }
    customData['theme'] = isDark ? 'dark' : 'light';
    customData['primaryColor'] = primary.value;

    // Update local state without full reload
    _userProfile = UserProfile(
      id: _userProfile!.id,
      userId: _userProfile!.userId,
      appId: _userProfile!.appId,
      displayName: _userProfile!.displayName,
      bio: _userProfile!.bio,
      avatarUrl: _userProfile!.avatarUrl,
      customDataJson: json.encode(customData),
    );

    final authClient = ref.read(authClientProvider);
    try {
      await authClient.updateUserProfile(_userProfile!);
    } catch (_) {}
  }

  void _logout() {
    ref.read(authProvider.notifier).logout();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    final primary = theme.primaryColor;

    if (_isLoading && _userProfile == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    // Avatar logic
    ImageProvider? bgImage;
    if (_imageFile != null) {
      bgImage = FileImage(File(_imageFile!.path));
    } else if (_userProfile?.avatarUrl != null &&
        _userProfile!.avatarUrl!.isNotEmpty) {
      bgImage = NetworkImage(_userProfile!.avatarUrl!);
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Profile'),
        bottom: TabBar(
          controller: _tabController,
          labelColor: primary,
          unselectedLabelColor: Colors.grey,
          indicatorColor: primary,
          tabs: const [
            Tab(icon: Icon(Icons.person_rounded), text: 'Personal'),
            Tab(icon: Icon(Icons.star_rounded), text: 'Premium'),
            Tab(icon: Icon(Icons.settings_rounded), text: 'Settings'),
          ],
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: IconButton(
              onPressed: _saveProfile,
              style: IconButton.styleFrom(
                  backgroundColor: primary.withOpacity(0.1)),
              icon: Icon(Icons.check_rounded, color: primary),
              tooltip: 'Save',
            ),
          )
        ],
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          // 1. Personal Info Tab
          ListView(
            padding: const EdgeInsets.all(24),
            children: [
              // Identity Section
              Center(
                child: Stack(
                  alignment: Alignment.bottomRight,
                  children: [
                    GestureDetector(
                      onTap: _pickImage,
                      child: CircleAvatar(
                        radius: 60,
                        backgroundColor: primary.withOpacity(0.1),
                        backgroundImage: bgImage,
                        child: bgImage == null
                            ? Icon(Icons.person_rounded,
                                size: 60, color: primary)
                            : null,
                      ),
                    ),
                    GestureDetector(
                      onTap: _pickImage,
                      child: Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                            color: primary,
                            shape: BoxShape.circle,
                            border: Border.all(
                                color:
                                    Theme.of(context).scaffoldBackgroundColor,
                                width: 3)),
                        child: const Icon(Icons.camera_alt_rounded,
                            color: Colors.white, size: 18),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              TextFormField(
                controller: _displayNameController,
                decoration: InputDecoration(
                  labelText: 'Display Name',
                  prefixIcon: const Icon(Icons.badge_outlined),
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface,
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _bioController,
                decoration: InputDecoration(
                  labelText: 'Bio',
                  prefixIcon: const Icon(Icons.info_outline_rounded),
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface,
                ),
                maxLines: 3,
              ),

              const SizedBox(height: 24),
              const Text('Demographics',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              const SizedBox(height: 16),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      initialValue: _userProfile?.dateOfBirth != null
                          ? "${_userProfile!.dateOfBirth!.year}-${_userProfile!.dateOfBirth!.month}-${_userProfile!.dateOfBirth!.day}"
                          : '',
                      readOnly: true,
                      decoration: InputDecoration(
                        labelText: 'Birthdate',
                        prefixIcon: const Icon(Icons.cake_outlined),
                        border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12)),
                        filled: true,
                        fillColor: theme.colorScheme.surface.withOpacity(0.5),
                      ),
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: TextFormField(
                      initialValue: _userProfile?.gender ?? '',
                      readOnly: true,
                      decoration: InputDecoration(
                        labelText: 'Gender',
                        prefixIcon: const Icon(Icons.transgender),
                        border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12)),
                        filled: true,
                        fillColor: theme.colorScheme.surface.withOpacity(0.5),
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 24),
              const Text('Contact & Verification',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              const SizedBox(height: 16),
              // Mobile
              TextFormField(
                controller: _mobileController,
                keyboardType: TextInputType.phone,
                decoration: InputDecoration(
                  labelText: 'Mobile Number',
                  prefixIcon: const Icon(Icons.phone_iphone_rounded),
                  suffixIcon: Icon(Icons.verified,
                      color: Colors.green), // Mock Verified
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface,
                ),
              ),
              const SizedBox(height: 16),
              // Email
              TextFormField(
                controller: _emailController,
                readOnly: true,
                decoration: InputDecoration(
                  labelText: 'Email Address',
                  prefixIcon: const Icon(Icons.email_outlined),
                  suffixIcon: Icon(Icons.verified,
                      color: Colors.green), // Mock Verified
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface.withOpacity(0.5),
                ),
              ),

              const SizedBox(height: 24),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text('Location',
                      style:
                          TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  TextButton.icon(
                    onPressed: _getCurrentLocation,
                    icon: const Icon(Icons.my_location),
                    label: const Text('Use GPS'),
                  )
                ],
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _countryController,
                      decoration: InputDecoration(
                        labelText: 'Country',
                        prefixIcon: const Icon(Icons.public),
                        border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12)),
                        filled: true,
                        fillColor: theme.colorScheme.surface,
                      ),
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: TextFormField(
                      controller: _cityController,
                      decoration: InputDecoration(
                        labelText: 'City',
                        prefixIcon: const Icon(Icons.location_city),
                        border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12)),
                        filled: true,
                        fillColor: theme.colorScheme.surface,
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 24),
              const Text('About Me',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              const SizedBox(height: 16),
              TextFormField(
                controller: _lookingForController,
                decoration: InputDecoration(
                  labelText: 'Looking For',
                  prefixIcon: const Icon(Icons.search),
                  hintText: 'e.g. A serious relationship, new friends...',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface,
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _interestsController,
                decoration: InputDecoration(
                  labelText: 'Interests',
                  prefixIcon: const Icon(Icons.favorite_border),
                  hintText: 'e.g. Hiking, Photography, Cooking...',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: theme.colorScheme.surface,
                ),
              ),
              const SizedBox(height: 48),
            ],
          ),

          // 2. Subscription Tab
          ListView(
            padding: const EdgeInsets.all(24),
            children: [
              Container(
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                    gradient: LinearGradient(
                        colors: [primary, primary.withOpacity(0.7)]),
                    borderRadius: BorderRadius.circular(16),
                    boxShadow: [
                      BoxShadow(
                          color: primary.withOpacity(0.3),
                          blurRadius: 10,
                          offset: const Offset(0, 5))
                    ]),
                child: const Column(
                  children: [
                    Icon(Icons.star_rounded, color: Colors.white, size: 48),
                    SizedBox(height: 16),
                    Text('Wissler Premium',
                        style: TextStyle(
                            color: Colors.white,
                            fontSize: 24,
                            fontWeight: FontWeight.bold)),
                    SizedBox(height: 8),
                    Text('Unlock unlimited swipes and see who likes you!',
                        style: TextStyle(color: Colors.white70),
                        textAlign: TextAlign.center),
                    SizedBox(height: 24),
                    Chip(
                        label: Text('Current Plan: Free',
                            style: TextStyle(color: Colors.black)),
                        backgroundColor: Colors.white)
                  ],
                ),
              ),
              const SizedBox(height: 24),
              const Text('Available Packages',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 16),
              if (_isLoadingPackages)
                const Center(
                    child: Padding(
                  padding: EdgeInsets.all(20.0),
                  child: CircularProgressIndicator(),
                ))
              else if (_packages.isEmpty)
                const Center(
                    child: Padding(
                  padding: EdgeInsets.all(20.0),
                  child: Text("No packages available.",
                      style: TextStyle(color: Colors.grey)),
                ))
              else
                ..._packages.map((pkg) {
                  final isSelected = _selectedPackageId == pkg.id;
                  final isBestValue =
                      pkg.name == "1 Month"; // Simple logic for now

                  return GestureDetector(
                    // Make the whole card tappable
                    onTap: () {
                      setState(() {
                        _selectedPackageId = pkg.id;
                      });
                    },
                    child: _PackageCard(
                      title: pkg.name,
                      price: '${pkg.price.toStringAsFixed(0)} ${pkg.currency}',
                      isSelected: isSelected,
                      primary: primary,
                      isBestValue: isBestValue,
                    ),
                  );
                }).toList(),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: _selectedPackageId == null
                    ? null
                    : () {
                        // Create Subscription Logic Here
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                              content: Text(
                                  'Selected Package: $_selectedPackageId')),
                        );
                      },
                style: ElevatedButton.styleFrom(
                  backgroundColor: primary,
                  disabledBackgroundColor: Colors.grey.withOpacity(0.3),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                ),
                child: const Text('Upgrade Now',
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              ),
            ],
          ),

          // 3. Settings Tab
          ListView(
            padding: const EdgeInsets.all(24),
            children: [
              const Text('Appearance',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 16),
              SwitchListTile(
                title: const Text('Dark Mode',
                    style: TextStyle(fontWeight: FontWeight.bold)),
                value: isDark,
                onChanged: (val) => _toggleTheme(val),
                secondary: Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                      color: isDark
                          ? Colors.grey[800]
                          : Colors.amber.withOpacity(0.2),
                      borderRadius: BorderRadius.circular(12)),
                  child: Icon(
                      isDark
                          ? Icons.dark_mode_rounded
                          : Icons.light_mode_rounded,
                      color: isDark ? Colors.white : Colors.amber),
                ),
              ),
              const SizedBox(height: 32),
              const Text('Notifications',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 16),
              SwitchListTile(
                title: const Text('Email Notifications'),
                subtitle: const Text('Receive updates via email'),
                secondary: const Icon(Icons.email_outlined),
                value: _emailNotify,
                onChanged: (val) {
                  setState(() => _emailNotify = val);
                },
              ),
              SwitchListTile(
                title: const Text('SMS Notifications'),
                subtitle: const Text('Receive updates via SMS'),
                secondary: const Icon(Icons.sms_outlined),
                value: _smsNotify,
                onChanged: (val) {
                  setState(() => _smsNotify = val);
                },
              ),
              SwitchListTile(
                title: const Text('Push Notifications'),
                subtitle: const Text('Receive push messages'),
                secondary: const Icon(Icons.notifications_active_outlined),
                value: _pushNotify,
                onChanged: (val) {
                  setState(() => _pushNotify = val);
                },
              ),
              const SizedBox(height: 32),
              const Divider(),
              const SizedBox(height: 16),
              ListTile(
                leading: const Icon(Icons.lock_outline),
                title: const Text('Privacy Settings'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () {},
              ),
              const SizedBox(height: 32),
              Center(
                child: TextButton.icon(
                  onPressed: _logout,
                  icon: const Icon(Icons.logout, color: Colors.red),
                  label: const Text('Log Out',
                      style: TextStyle(color: Colors.red)),
                ),
              )
            ],
          ),
        ],
      ),
    );
  }
}

class _PackageCard extends StatelessWidget {
  final String title;
  final String price;
  final bool isSelected;
  final Color primary;
  final bool isBestValue;

  const _PackageCard({
    required this.title,
    required this.price,
    required this.isSelected,
    required this.primary,
    this.isBestValue = false,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isSelected ? primary : Colors.grey.withOpacity(0.3),
          width: isSelected ? 2 : 1,
        ),
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: Icon(
          isSelected ? Icons.check_circle : Icons.radio_button_unchecked,
          color: isSelected ? primary : Colors.grey,
        ),
        title: Text(title,
            style: TextStyle(
                fontWeight: FontWeight.bold,
                color: isSelected ? primary : null)),
        subtitle: isBestValue
            ? Text('Best Value',
                style: TextStyle(
                    color: primary, fontWeight: FontWeight.bold, fontSize: 12))
            : null,
        trailing: Text(price,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
      ),
    );
  }
}
