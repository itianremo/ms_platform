import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import '../../widgets/custom_toast.dart';
import 'package:api_client/api_client.dart';
import 'package:geolocator/geolocator.dart';

class FilterModal extends ConsumerStatefulWidget {
  final Function() onApply;
  final ScrollController? scrollController;

  const FilterModal({super.key, required this.onApply, this.scrollController});

  @override
  ConsumerState<FilterModal> createState() => _FilterModalState();
}

class _FilterModalState extends ConsumerState<FilterModal> {
  RangeValues _ageRange = const RangeValues(18, 50);
  String? _selectedCountry;
  String? _interestedIn;

  // Advanced Filters
  double _distanceKm = 100.0;
  String _drinking = 'ALL';
  String _smoking = 'ALL';
  String _education = 'ALL';
  String _eyeColor = 'ALL';
  String _hairColor = 'ALL';
  String _religion = 'ALL';
  String _intent = 'ALL';
  String _lifestyle = 'ALL';

  bool _isLoading = true;
  UserProfile? _profile;
  Map<String, dynamic> _customData = {};

  final String _demoUserId =
      "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // Same hardcoded ID as WisslerScreen

  @override
  void initState() {
    super.initState();
    _loadPreferences();
  }

  Future<void> _loadPreferences() async {
    try {
      final authRepo = ref.read(authRepositoryProvider);
      final token = authRepo.getAccessToken();

      if (token == null) {
        throw Exception("User not logged in");
      }

      final parts = token.split('.');
      if (parts.length != 3) throw Exception("Invalid token");

      final payload = parts[1];
      final normalized = base64Url.normalize(payload);
      final resp = utf8.decode(base64Url.decode(normalized));
      final decodedToken = json.decode(resp);

      final userId =
          decodedToken['sub'] ?? decodedToken['id'] ?? decodedToken['userId'];

      if (userId == null) {
        throw Exception("User ID not found in token");
      }

      final client = ref.read(authClientProvider);
      final profile = await client.getUserProfile(
          userId, '22222222-2222-2222-2222-222222222222');

      if (mounted) {
        setState(() {
          if (profile != null) {
            _profile = profile;
            if (profile.customDataJson != null) {
              try {
                _customData = json.decode(profile.customDataJson!);
                final prefs =
                    _customData['preferences'] as Map<String, dynamic>?;
                if (prefs != null) {
                  if (prefs['minAge'] != null && prefs['maxAge'] != null) {
                    double min = (prefs['minAge'] as num).toDouble();
                    double max = (prefs['maxAge'] as num).toDouble();
                    _ageRange = RangeValues(min, max);
                  }
                  _selectedCountry = prefs['country'];
                  _interestedIn =
                      prefs['interestedIn'] ?? _customData['interestedIn'];

                  _distanceKm =
                      (prefs['distanceKm'] as num?)?.toDouble() ?? 100.0;
                  _drinking = prefs['drinking'] as String? ?? 'ALL';
                  _smoking = prefs['smoking'] as String? ?? 'ALL';
                  _education = prefs['education'] as String? ?? 'ALL';
                  _eyeColor = prefs['eyeColor'] as String? ?? 'ALL';
                  _hairColor = prefs['hairColor'] as String? ?? 'ALL';
                  _religion = prefs['religion'] as String? ?? 'ALL';
                  _intent = prefs['intent'] as String? ?? 'ALL';
                  _lifestyle = prefs['lifestyle'] as String? ?? 'ALL';
                }
              } catch (_) {}
            }
          }
          _isLoading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<Position?> _determinePosition() async {
    bool serviceEnabled;
    LocationPermission permission;

    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      return null;
    }

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) {
        return null;
      }
    }

    if (permission == LocationPermission.deniedForever) {
      return null;
    }

    return await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.medium);
  }

  Future<void> _savePreferences() async {
    if (_profile == null) return;
    setState(() => _isLoading = true);

    try {
      Position? position = await _determinePosition();

      final client = ref.read(authClientProvider);

      // Update preferences locally
      _customData['preferences'] = {
        'minAge': _ageRange.start.round(),
        'maxAge': _ageRange.end.round(),
        'country': _selectedCountry,
        'interestedIn': _interestedIn,
        'distanceKm': _distanceKm,
        'drinking': _drinking,
        'smoking': _smoking,
        'education': _education,
        'eyeColor': _eyeColor,
        'hairColor': _hairColor,
        'religion': _religion,
        'intent': _intent,
        'lifestyle': _lifestyle,
        if (position != null) 'latitude': position.latitude,
        if (position != null) 'longitude': position.longitude,
      };

      if (_interestedIn != null) {
        _customData['interestedIn'] = _interestedIn;
      }

      final newCustomDataJson = json.encode(_customData);

      final updatedProfile = UserProfile(
        id: _profile!.id,
        userId: _profile!.userId,
        appId: _profile!.appId,
        displayName: _profile!.displayName,
        bio: _profile!.bio,
        avatarUrl: _profile!.avatarUrl,
        customDataJson: newCustomDataJson,
        dateOfBirth: _profile!.dateOfBirth,
        gender: _profile!.gender,
      );

      await client.updateUserProfile(updatedProfile);

      if (mounted) {
        CustomToast.show(context, "Filter applied");
        widget.onApply();
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        CustomToast.show(context, "Failed to save preferences.");
      }
    }
  }

  Future<void> _resetFilters() async {
    setState(() => _isLoading = true);
    await _loadPreferences();
    if (mounted) {
      CustomToast.show(context, "Filters restored");
      setState(() => _isLoading = false);
    }
  }

  Widget _buildDropdown(String label, String value, List<String> options,
      Function(String?) onChanged) {
    if (!options.contains('ALL')) {
      options.insert(0, 'ALL');
    }
    return Padding(
      padding: const EdgeInsets.only(bottom: 16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            initialValue: value,
            decoration: const InputDecoration(
                border: OutlineInputBorder(),
                contentPadding: EdgeInsets.symmetric(horizontal: 10)),
            items: options
                .map((o) => DropdownMenuItem(value: o, child: Text(o)))
                .toList(),
            onChanged: onChanged,
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Container(
          height: 300,
          decoration: const BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
          child: const Center(child: CircularProgressIndicator()));
    }

    return Container(
      decoration: const BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      child: SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Sticky Header
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 24, 24, 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text('Filters',
                      style:
                          TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 8),
                  Container(
                    height: 3,
                    width: 40,
                    color: Theme.of(context).primaryColor,
                  ),
                ],
              ),
            ),

            // Scrollable Content
            Expanded(
              child: SingleChildScrollView(
                controller: widget.scrollController,
                padding: const EdgeInsets.symmetric(horizontal: 24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Distance Slider
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text('Maximum Distance',
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        Text('${_distanceKm.round()} km',
                            style: TextStyle(
                                color: Theme.of(context).primaryColor,
                                fontWeight: FontWeight.bold)),
                      ],
                    ),
                    Slider(
                      value: _distanceKm,
                      min: 5,
                      max:
                          200, // 200 represents Global/No limit conceptually, but sticking to km
                      activeColor: Theme.of(context).primaryColor,
                      onChanged: (val) => setState(() => _distanceKm = val),
                    ),
                    const SizedBox(height: 16),

                    // Interested In
                    const Text('Interested In',
                        style: TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(height: 10),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: ['Male', 'Female', 'All'].map((option) {
                        final isSelected = _interestedIn == option;
                        return ChoiceChip(
                          label: Text(option),
                          selected: isSelected,
                          selectedColor:
                              Theme.of(context).primaryColor.withOpacity(0.2),
                          onSelected: (selected) {
                            setState(
                                () => _interestedIn = selected ? option : null);
                          },
                        );
                      }).toList(),
                    ),
                    const SizedBox(height: 24),

                    // Age Range
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text('Age Range',
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        Text(
                            '${_ageRange.start.round()} - ${_ageRange.end.round()}',
                            style: TextStyle(
                                color: Theme.of(context).primaryColor,
                                fontWeight: FontWeight.bold)),
                      ],
                    ),
                    RangeSlider(
                      values: _ageRange,
                      min: 18,
                      max: 100,
                      divisions: 82,
                      activeColor: Theme.of(context).primaryColor,
                      labels: RangeLabels(_ageRange.start.round().toString(),
                          _ageRange.end.round().toString()),
                      onChanged: (values) => setState(() => _ageRange = values),
                    ),
                    const SizedBox(height: 24),

                    // Country DDL
                    Padding(
                        padding: const EdgeInsets.only(bottom: 16.0),
                        child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text('Country',
                                  style:
                                      TextStyle(fontWeight: FontWeight.bold)),
                              const SizedBox(height: 8),
                              DropdownButtonFormField<String>(
                                initialValue: _selectedCountry,
                                decoration: const InputDecoration(
                                    border: OutlineInputBorder(),
                                    contentPadding:
                                        EdgeInsets.symmetric(horizontal: 10)),
                                items: [
                                  'ALL',
                                  'Egypt',
                                  'Germany',
                                  'USA',
                                  'France',
                                  'UK',
                                  'Spain'
                                ]
                                    .map((c) => DropdownMenuItem(
                                        value: c == 'ALL' ? null : c,
                                        child: Text(c)))
                                    .toList(),
                                onChanged: (v) =>
                                    setState(() => _selectedCountry = v),
                              ),
                            ])),

                    // Dynamic Attribute DDLs
                    _buildDropdown(
                        'Drinking',
                        _drinking,
                        ['Never', 'Socially', 'Regularly'],
                        (v) => setState(() => _drinking = v!)),
                    _buildDropdown(
                        'Smoking',
                        _smoking,
                        ['Never', 'Socially', 'Regularly'],
                        (v) => setState(() => _smoking = v!)),
                    _buildDropdown(
                        'Education',
                        _education,
                        ['High School', 'Bachelors', 'Masters', 'PhD'],
                        (v) => setState(() => _education = v!)),
                    _buildDropdown(
                        'Eye Color',
                        _eyeColor,
                        ['Brown', 'Blue', 'Green', 'Hazel'],
                        (v) => setState(() => _eyeColor = v!)),
                    _buildDropdown(
                        'Hair Color',
                        _hairColor,
                        ['Black', 'Brown', 'Blonde', 'Red', 'Other'],
                        (v) => setState(() => _hairColor = v!)),
                    _buildDropdown(
                        'Religion',
                        _religion,
                        [
                          'Muslim',
                          'Christian',
                          'Jewish',
                          'Hindu',
                          'Buddhist',
                          'Spiritual',
                          'None'
                        ],
                        (v) => setState(() => _religion = v!)),
                    _buildDropdown(
                        'Intent',
                        _intent,
                        ['Casual', 'Dating', 'Marriage', 'Friends'],
                        (v) => setState(() => _intent = v!)),
                    _buildDropdown(
                        'Lifestyle',
                        _lifestyle,
                        ['Active', 'Moderate', 'Sedentary'],
                        (v) => setState(() => _lifestyle = v!)),

                    const SizedBox(height: 32),
                  ],
                ),
              ),
            ),

            // Sticky Footer / Apply Button
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 16, 24, 16),
              child: SizedBox(
                width: double.infinity,
                height: 50,
                child: Row(
                  children: [
                    // Reset Button
                    Container(
                      margin: const EdgeInsets.only(right: 12),
                      decoration: BoxDecoration(
                        color: Colors.grey.shade200,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: IconButton(
                        icon: const Icon(Icons.refresh, color: Colors.black87),
                        onPressed: _isLoading ? null : _resetFilters,
                        tooltip: 'Reset to last saved',
                      ),
                    ),
                    // Apply Button
                    Expanded(
                      child: ElevatedButton(
                        style: ElevatedButton.styleFrom(
                            backgroundColor: Theme.of(context).primaryColor,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12))),
                        onPressed: _isLoading ? null : _savePreferences,
                        child: _isLoading
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(
                                    color: Colors.white, strokeWidth: 2))
                            : const Text('Apply filters',
                                style: TextStyle(fontSize: 18)),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
