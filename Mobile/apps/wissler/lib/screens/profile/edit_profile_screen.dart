import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'package:api_client/api_client.dart';
import 'verification_screen.dart';

class EditProfileScreen extends ConsumerStatefulWidget {
  final UserProfile userProfile;
  final List<Country> countries;
  final List<City> cities;
  final VoidCallback? onBack;

  const EditProfileScreen({
    super.key,
    required this.userProfile,
    required this.countries,
    required this.cities,
    this.onBack,
  });

  @override
  ConsumerState<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends ConsumerState<EditProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _displayNameController;
  late TextEditingController _bioController;
  late TextEditingController _mobileController;
  late TextEditingController _emailController;

  // DDLs
  DateTime? _birthDate;
  String? _gender;
  String? _selectedCountryId;
  String? _selectedCityId;

  // Questionnaire
  String? _weight, _height, _hairColor, _eyeColor, _smoking, _drinking;

  // Photos
  final ImagePicker _picker = ImagePicker();
  List<String> _photos = [];

  bool _isLoading = false;
  List<City> _currentCities = [];

  @override
  void initState() {
    super.initState();
    final p = widget.userProfile;
    _displayNameController = TextEditingController(text: p.displayName);
    _bioController = TextEditingController(text: p.bio);
    _emailController = TextEditingController(); // Filled from JWT usually

    // Parse Custom Data
    _currentCities = widget.cities;
    _parseCustomData(p.customDataJson);

    _birthDate = p.dateOfBirth;
    _gender = p.gender;

    // Safety check for mobile controller if not in custom data
    if (!(_mobileController is TextEditingController)) {
      _mobileController = TextEditingController();
    }
  }

  void _parseCustomData(String? jsonStr) {
    if (jsonStr == null) {
      _mobileController = TextEditingController();
      return;
    }
    try {
      final data = json.decode(jsonStr);
      _mobileController = TextEditingController(text: data['mobile'] ?? '');
      _selectedCountryId = data['countryId'];
      _selectedCityId = data['cityId'];

      _weight = data['weight'];
      _height = data['height'];
      _hairColor = data['hairColor'];
      _eyeColor = data['eyeColor'];
      _smoking = data['smoking'];
      _drinking = data['drinking'];

      if (data.containsKey('photos')) {
        _photos = List<String>.from(data['photos']);
      }
    } catch (_) {
      _mobileController = TextEditingController();
    }
  }

  @override
  void dispose() {
    _displayNameController.dispose();
    _bioController.dispose();
    _mobileController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);
    try {
      final client = ref.read(authClientProvider);

      Map<String, dynamic> customData = {};
      // Inherit existing data to not lose coins/verification status
      if (widget.userProfile.customDataJson != null) {
        try {
          customData = json.decode(widget.userProfile.customDataJson!);
        } catch (_) {}
      }

      customData['mobile'] = _mobileController.text;
      customData['countryId'] = _selectedCountryId;
      customData['cityId'] = _selectedCityId;
      customData['weight'] = _weight;
      customData['height'] = _height;
      customData['hairColor'] = _hairColor;
      customData['eyeColor'] = _eyeColor;
      customData['smoking'] = _smoking;
      customData['drinking'] = _drinking;
      customData['photos'] = _photos;

      final updated = UserProfile(
          id: widget.userProfile.id,
          userId: widget.userProfile.userId,
          appId: widget.userProfile.appId,
          displayName: _displayNameController.text,
          bio: _bioController.text,
          avatarUrl:
              _photos.isNotEmpty ? _photos.first : widget.userProfile.avatarUrl,
          customDataJson: json.encode(customData),
          dateOfBirth: _birthDate,
          gender: _gender);

      await client.updateUserProfile(updated);
      if (mounted) {
        if (widget.onBack != null) {
          widget.onBack!();
        } else {
          Navigator.pop(context, true);
        }
      }
    } catch (e) {
      if (mounted)
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Error: $e')));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _pickPhoto() async {
    final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
    if (image != null) {
      // In real app, upload here. Mocking with Unsplash for now.
      setState(() => _photos.add(
          'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=500&auto=format&fit=crop&q=60'));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(
          leading: IconButton(
              icon: const Icon(Icons.close),
              onPressed: () {
                if (widget.onBack != null) {
                  widget.onBack!();
                } else {
                  Navigator.pop(context);
                }
              }),
          title: const Text("Edit Profile"),
          actions: [
            IconButton(
                icon: const Icon(Icons.check, color: Colors.green),
                onPressed: _save)
          ],
        ),
        body: Form(
            key: _formKey,
            child: ListView(padding: const EdgeInsets.all(20), children: [
              if (_isLoading) const LinearProgressIndicator(),
              const SizedBox(height: 10),
              // Photos
              SizedBox(
                  height: 220,
                  child: ReorderableListView(
                      scrollDirection: Axis.horizontal,
                      onReorder: (oldIndex, newIndex) {
                        setState(() {
                          if (oldIndex < newIndex) newIndex -= 1;
                          final item = _photos.removeAt(oldIndex);
                          _photos.insert(newIndex, item);
                        });
                      },
                      children: [
                        for (int i = 0; i < _photos.length; i++)
                          Container(
                            key: ValueKey(_photos[i]),
                            width: 150,
                            margin: const EdgeInsets.only(right: 10),
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(10),
                              image: DecorationImage(
                                  image: NetworkImage(_photos[i]),
                                  fit: BoxFit.cover),
                            ),
                            child: Stack(children: [
                              Positioned(
                                  top: 5,
                                  right: 5,
                                  child: GestureDetector(
                                      onTap: () =>
                                          setState(() => _photos.removeAt(i)),
                                      child: const CircleAvatar(
                                          radius: 12,
                                          backgroundColor: Colors.red,
                                          child: Icon(Icons.close,
                                              size: 14, color: Colors.white)))),
                              Positioned(
                                  bottom: 0,
                                  left: 0,
                                  right: 0,
                                  child: Container(
                                      color: Colors.black45,
                                      padding: const EdgeInsets.all(4),
                                      child: const Text("Pending Approval",
                                          style: TextStyle(
                                              color: Colors.white,
                                              fontSize: 10),
                                          textAlign: TextAlign.center)))
                            ]),
                          ),
                        Container(
                            key: const ValueKey('add_btn'),
                            width: 100,
                            color: Colors.grey[200],
                            child: IconButton(
                                icon: const Icon(Icons.add_a_photo),
                                onPressed: _pickPhoto))
                      ])),
              const SizedBox(height: 20),
              // Basic Info
              const Text("Basic Info",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 10),
              TextFormField(
                  controller: _displayNameController,
                  validator: (v) => v!.isEmpty ? "Required" : null,
                  decoration: const InputDecoration(
                      labelText: "Display Name", border: OutlineInputBorder())),
              const SizedBox(height: 10),
              TextFormField(
                  controller: _bioController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                      labelText: "Bio", border: OutlineInputBorder())),
              const SizedBox(height: 20),
              // Verification
              const Text("Contact & Verification",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 10),
              TextFormField(
                  controller: _mobileController,
                  decoration: InputDecoration(
                      labelText: "Mobile",
                      border: const OutlineInputBorder(),
                      suffixIcon: TextButton(
                          onPressed: () {
                            Navigator.push(
                                context,
                                MaterialPageRoute(
                                    builder: (_) => VerificationScreen(
                                        userId: widget.userProfile.userId,
                                        type: 'sms')));
                          },
                          child: const Text("Verify")))),
              const SizedBox(height: 10),
              // DDLs
              const SizedBox(height: 20),
              const Text("Details",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              const SizedBox(height: 10),
              Row(children: [
                Expanded(
                    child: DropdownButtonFormField<String>(
                        value: _gender,
                        items: ['Male', 'Female', 'Prefer not to answer']
                            .map((g) =>
                                DropdownMenuItem(value: g, child: Text(g)))
                            .toList(),
                        onChanged: (v) => setState(() => _gender = v),
                        decoration: const InputDecoration(
                            labelText: "Gender",
                            border: OutlineInputBorder()))),
                const SizedBox(width: 10),
                Expanded(
                    child: DropdownButtonFormField<String>(
                        value: _selectedCountryId,
                        items: widget.countries
                            .map((c) => DropdownMenuItem(
                                value: c.id, child: Text(c.name)))
                            .toList(),
                        onChanged: (v) => setState(() => _selectedCountryId =
                            v), // Load cities logic omitted for brevity, assumes pass in
                        decoration: const InputDecoration(
                            labelText: "Country",
                            border: OutlineInputBorder()))),
              ]),
            ])));
  }
}
