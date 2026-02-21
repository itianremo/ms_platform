import 'package:api_client/api_client.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_core/providers/auth_provider.dart';
import 'dart:convert';
import '../../widgets/custom_toast.dart';
import '../matches_and_chat/chat_detail_screen.dart';

class UserDetailsScreen extends ConsumerStatefulWidget {
  final Recommendation?
      profile; // Make nullable to support direct UserProfile passing
  final UserProfile?
      userProfile; // Support passing full profile directly (e.g. Preview)
  final bool isMatched;
  final VoidCallback? onBack;
  final Function(String)? onSwipe;

  const UserDetailsScreen(
      {super.key,
      this.profile,
      this.userProfile,
      this.isMatched = false,
      this.onBack,
      this.onSwipe})
      : assert(profile != null || userProfile != null);

  @override
  ConsumerState<UserDetailsScreen> createState() => _UserDetailsScreenState();
}

class _UserDetailsScreenState extends ConsumerState<UserDetailsScreen> {
  UserProfile? _fullProfile;
  bool _isLoading = true;
  int _currentImageIndex = 0;

  // Custom Data Fields
  List<Map<String, dynamic>> _photos = []; // [{url, isVerified, isApproved}]
  Map<String, dynamic> _attributes = {};
  bool _isPhoneVerified = false;
  bool _isEmailVerified = false;

  late PageController _pageController;

  String? _existingReportReason;
  String? _existingReportComment;

  @override
  void initState() {
    super.initState();
    _pageController = PageController();
    _loadData();
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  void _nextImage(int count) {
    if (_currentImageIndex < count - 1) {
      _pageController.nextPage(
          duration: const Duration(milliseconds: 300), curve: Curves.easeInOut);
    }
  }

  void _prevImage() {
    if (_currentImageIndex > 0) {
      _pageController.previousPage(
          duration: const Duration(milliseconds: 300), curve: Curves.easeInOut);
    }
  }

  void _parseData() {
    if (_fullProfile?.customDataJson == null) return;
    try {
      final data = json.decode(_fullProfile!.customDataJson!);
      _attributes = data;

      // Parse Verification
      _isPhoneVerified = data['phoneVerified'] == true;
      _isEmailVerified = data['emailVerified'] == true;

      // Extract explicitly requested foundational data for the UI
      if (data['interestedIn'] != null) {
        _attributes['interestedIn'] = data['interestedIn'];
      }

      // Parse Photos with Strict Visibility Rules for "Other" profiles
      if (data['photos'] != null) {
        _photos = (data['photos'] as List)
            .map((p) {
              if (p is String) {
                return {
                  'url': p,
                  'isVerified': false,
                  'isApproved': true,
                  'isActive': true
                }; // Backward compat
              }
              return p as Map<String, dynamic>;
            })
            .where((p) =>
                (p['isApproved'] == true || p['isVerified'] == true) &&
                p['isActive'] ==
                    true) // EITHER verified OR approved, AND active
            .toList();
      }
    } catch (_) {}
  }

  Future<void> _loadData() async {
    final client = ref.read(authClientProvider);
    final authRepo = ref.read(authRepositoryProvider);

    try {
      if (widget.userProfile != null) {
        _fullProfile = widget.userProfile;
        _parseData();
      } else if (widget.profile != null) {
        final profile = await client.getUserProfile(
            widget.profile!.userId, "22222222-2222-2222-2222-222222222222");
        if (profile != null) {
          _fullProfile = profile;
          _parseData();
        }
      }

      final token = authRepo.getAccessToken();
      String? currentUserId;
      if (token != null) {
        final parts = token.split('.');
        if (parts.length == 3) {
          final payload = parts[1];
          final normalized = base64Url.normalize(payload);
          final resp = utf8.decode(base64Url.decode(normalized));
          final decodedToken = json.decode(resp);
          currentUserId = decodedToken['sub'] ??
              decodedToken['id'] ??
              decodedToken['userId'];
        }
      }

      final targetId =
          widget.profile?.userId ?? _fullProfile?.userId.toString();
      if (targetId != null && targetId.isNotEmpty && currentUserId != null) {
        final existingReport =
            await client.getExistingReportReason(currentUserId, targetId);
        if (existingReport != null) {
          _existingReportReason = existingReport['reason'];
          _existingReportComment = existingReport['comment'];
        }
      }
    } catch (e) {
      print("Error loading profile or report: $e");
    }

    if (mounted) {
      setState(() {
        _isLoading = false;
      });
    }
  }

  // Logic: Phone + Email + All Available Photos Verified
  bool get _isWisslerVerified {
    // Basic checks
    if (!_isPhoneVerified || !_isEmailVerified) return false;
    if (_photos.isEmpty) return false; // Must have photos to be verified

    // Check all photos
    return _photos.every((p) => p['isVerified'] == true);
  }

  @override
  Widget build(BuildContext context) {
    final primary = Theme.of(context).primaryColor;

    // Resolve display data
    final displayName =
        _fullProfile?.displayName ?? widget.profile?.displayName ?? 'User';
    final age = _fullProfile?.dateOfBirth != null
        ? _calculateAge(_fullProfile!.dateOfBirth!)
        : (widget.profile?.age.toString() ?? 'N/A');
    final genderText = _fullProfile?.gender ?? "Not specified";
    final city =
        _attributes['cityId'] ?? widget.profile?.city ?? 'Unknown City';
    final country = _attributes['countryId'] ??
        widget.profile?.country ??
        'Unknown Country';
    final bio = _fullProfile?.bio ?? "No bio available.";

    // Photos logic: Use fetched photos, or fallback to avatarUrl
    List<Map<String, dynamic>> displayPhotos = [];
    if (_photos.isNotEmpty) {
      displayPhotos = _photos;
    } else {
      // Fallback to single avatar
      final fallback = widget.profile?.avatarUrl ?? _fullProfile?.avatarUrl;
      if (fallback != null && fallback.isNotEmpty) {
        displayPhotos
            .add({'url': fallback, 'isVerified': false, 'isApproved': true});
      }
    }

    return Scaffold(
      body: Stack(
        children: [
          CustomScrollView(
            slivers: [
              SliverAppBar(
                expandedHeight:
                    MediaQuery.of(context).size.height * 0.55, // 55% height
                pinned: true,
                stretch: true,
                backgroundColor: primary,
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back,
                      color: Colors.white,
                      shadows: [Shadow(color: Colors.black, blurRadius: 10)]),
                  onPressed: () {
                    if (widget.onBack != null) {
                      widget.onBack!();
                    } else {
                      Navigator.pop(context);
                    }
                  },
                ),
                flexibleSpace: FlexibleSpaceBar(
                  background: Stack(
                    fit: StackFit.expand,
                    children: [
                      PageView(
                        physics:
                            const BouncingScrollPhysics(), // Ensure scrolling is enabled
                        controller: _pageController,
                        onPageChanged: (index) =>
                            setState(() => _currentImageIndex = index),
                        children: displayPhotos
                            .map((p) => Stack(
                                  fit: StackFit.expand,
                                  children: [
                                    Image.network(p['url'], fit: BoxFit.cover),
                                    if (p['isVerified'] == true)
                                      Positioned(
                                          top: 30 +
                                              MediaQuery.of(context)
                                                  .padding
                                                  .top,
                                          right: 20,
                                          child: const Icon(Icons.verified,
                                              color: Colors.orange,
                                              size: 28,
                                              shadows: [
                                                Shadow(
                                                    color: Colors.black54,
                                                    blurRadius: 5)
                                              ]))
                                  ],
                                ))
                            .toList(),
                      ),
                      // Tap Zones Overlay
                      Row(children: [
                        Expanded(
                            child: GestureDetector(
                                behavior: HitTestBehavior.translucent,
                                onTap: _prevImage,
                                child: Container(color: Colors.transparent))),
                        Expanded(
                            child: GestureDetector(
                                behavior: HitTestBehavior.translucent,
                                onTap: () => _nextImage(displayPhotos.length),
                                child: Container(color: Colors.transparent))),
                      ]),
                      // Indicators
                      if (displayPhotos.length > 1)
                        Positioned(
                          top: 10 + MediaQuery.of(context).padding.top,
                          left: 10,
                          right: 10,
                          child: Row(
                            children:
                                List.generate(displayPhotos.length, (index) {
                              return Expanded(
                                child: Container(
                                  height: 4,
                                  margin:
                                      const EdgeInsets.symmetric(horizontal: 2),
                                  decoration: BoxDecoration(
                                      color: index == _currentImageIndex
                                          ? Colors.white
                                          : Colors.white24,
                                      borderRadius: BorderRadius.circular(2),
                                      boxShadow: const [
                                        BoxShadow(
                                            color: Colors.black26,
                                            blurRadius: 2)
                                      ]),
                                ),
                              );
                            }),
                          ),
                        ),
                    ],
                  ),
                ),
              ),
              SliverToBoxAdapter(
                child: Transform.translate(
                  offset: const Offset(0, -30),
                  child: Container(
                    decoration: const BoxDecoration(
                      color: Colors.white,
                      borderRadius:
                          BorderRadius.vertical(top: Radius.circular(40)),
                      boxShadow: [
                        BoxShadow(
                            color: Colors.black12,
                            blurRadius: 10,
                            offset: Offset(0, -5))
                      ],
                    ),
                    padding: const EdgeInsets.fromLTRB(24, 40, 24, 120),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Name & Age Row
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              "$displayName, $age",
                              style: const TextStyle(
                                fontSize: 28,
                                fontWeight: FontWeight.bold,
                                color: Colors.black87,
                              ),
                            ),
                            if (_isWisslerVerified)
                              Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 8, vertical: 4),
                                decoration: BoxDecoration(
                                    color: Colors.orange[50],
                                    borderRadius: BorderRadius.circular(10),
                                    border: Border.all(
                                        color: Colors.orange.withOpacity(0.3))),
                                child: const Row(children: [
                                  Text("Verified by Wissler",
                                      style: TextStyle(
                                          color: Colors.orange,
                                          fontWeight: FontWeight.bold,
                                          fontSize: 10)),
                                  SizedBox(width: 4),
                                  Icon(Icons.gpp_good,
                                      color: Colors.orange, size: 14)
                                ]),
                              )
                            else if (widget.profile?.isVerified == true)
                              // Fallback for old/simple verification
                              const Icon(Icons.gpp_good,
                                  color: Colors.orange, size: 28)
                          ],
                        ),
                        // Location & Geometry
                        const SizedBox(height: 5),
                        Row(
                          children: [
                            Icon(Icons.location_on,
                                size: 16, color: Colors.grey[600]),
                            const SizedBox(width: 4),
                            Text(
                              "$city, $country",
                              style: TextStyle(
                                  color: Colors.grey[700], fontSize: 16),
                            ),
                          ],
                        ),

                        // Core Demographics
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: [
                            _buildAttributeChip(Icons.cake, "Age $age"),
                            _buildAttributeChip(Icons.person, genderText),
                            if (_attributes['interestedIn'] != null)
                              _buildAttributeChip(Icons.favorite_border,
                                  "Interested: ${_attributes['interestedIn']}"),
                          ],
                        ),

                        const Divider(height: 40),

                        // About
                        const Text("About",
                            style: TextStyle(
                                fontSize: 18, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 10),
                        Text(bio,
                            style: const TextStyle(
                                fontSize: 16,
                                height: 1.5,
                                color: Colors.black54)),

                        const Divider(height: 40),

                        // Details Grid
                        const Text("Details",
                            style: TextStyle(
                                fontSize: 18, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 15),
                        Wrap(
                          spacing: 10,
                          runSpacing: 10,
                          children: [
                            _buildAttributeChip(Icons.info_outline,
                                "Education: ${_attributes['education'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Height: ${_attributes['height'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Drinking: ${_attributes['drinking'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Smoking: ${_attributes['smoking'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Eye Color: ${_attributes['eyeColor'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Hair Color: ${_attributes['hairColor'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Religion: ${_attributes['religion'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Lifestyle: ${_attributes['lifestyle'] ?? 'N/A'}"),
                            _buildAttributeChip(Icons.info_outline,
                                "Intent: ${_attributes['intent'] ?? 'N/A'}"),
                          ],
                        ),

                        // Footer Actions
                        const SizedBox(height: 40),
                        Center(
                          child: Column(
                            children: [
                              TextButton.icon(
                                  onPressed: _showReportDialog,
                                  icon: Icon(Icons.flag,
                                      color: _existingReportReason != null
                                          ? Colors.orange
                                          : Colors.grey),
                                  label: Text(
                                      _existingReportReason != null
                                          ? "Edit Report"
                                          : "Report Account",
                                      style: TextStyle(
                                          color: _existingReportReason != null
                                              ? Colors.orange
                                              : Colors.grey))),
                              if (widget.isMatched) ...[
                                const SizedBox(height: 10),
                                TextButton.icon(
                                    onPressed: () {},
                                    icon: const Icon(Icons.block,
                                        color: Colors.red),
                                    label: const Text("Unmatch",
                                        style: TextStyle(color: Colors.red))),
                              ]
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
          if (_isLoading) const Center(child: CircularProgressIndicator()),
        ],
      ),
      // Fixed Bottom Action Bar (Safe Area Checked)
      bottomNavigationBar: widget.userProfile != null
          ? null
          : SafeArea(child: _buildBottomActionBar(context)),
    );
  }

  void _showReportDialog() async {
    final client = ref.read(authClientProvider);
    final authRepo = ref.read(authRepositoryProvider);

    if (!mounted) return;

    // Decode token for currentUserId
    final token = authRepo.getAccessToken();
    String? currentUserId;
    if (token != null) {
      final parts = token.split('.');
      if (parts.length == 3) {
        final payload = parts[1];
        final normalized = base64Url.normalize(payload);
        final resp = utf8.decode(base64Url.decode(normalized));
        final decodedToken = json.decode(resp);
        currentUserId =
            decodedToken['sub'] ?? decodedToken['id'] ?? decodedToken['userId'];
      }
    }

    if (currentUserId == null) {
      CustomToast.show(context, "User session not found.");
      return;
    }

    // Show loading indicator
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => const Center(child: CircularProgressIndicator()),
    );

    final reasons = await client.getReportReasons();
    if (!mounted) return;
    Navigator.pop(context); // pop loading

    showDialog(
        context: context,
        builder: (context) {
          String? selectedReason = _existingReportReason;
          final commentController =
              TextEditingController(text: _existingReportComment);

          return StatefulBuilder(builder: (context, setState) {
            return AlertDialog(
              title: Text(_existingReportReason != null
                  ? "Edit Report"
                  : "Report Account"),
              content: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text(
                        "Please select a reason for reporting this profile."),
                    const SizedBox(height: 16),
                    DropdownButtonFormField<String>(
                      decoration:
                          const InputDecoration(border: OutlineInputBorder()),
                      hint: const Text("Select reason"),
                      initialValue: selectedReason,
                      items: reasons
                          .map(
                              (r) => DropdownMenuItem(value: r, child: Text(r)))
                          .toList(),
                      onChanged: (val) => setState(() => selectedReason = val),
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: commentController,
                      decoration: const InputDecoration(
                        border: OutlineInputBorder(),
                        hintText: "Optional comment...",
                      ),
                      maxLines: 3,
                    )
                  ],
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () {
                    commentController.dispose();
                    Navigator.pop(context);
                  },
                  child: const Text("Cancel"),
                ),
                ElevatedButton(
                  onPressed: selectedReason == null
                      ? null
                      : () async {
                          Navigator.pop(context); // close dialog
                          final success = await client.reportUser(
                              currentUserId!,
                              widget.profile?.userId ??
                                  _fullProfile!.userId.toString(),
                              selectedReason!,
                              commentController.text);
                          if (mounted) {
                            if (success) {
                              this.setState(() {
                                _existingReportReason = selectedReason;
                                _existingReportComment = commentController.text;
                              });
                            }
                            commentController.dispose();
                            CustomToast.show(
                                context,
                                success
                                    ? "Report submitted successfully."
                                    : "Failed to submit report.");
                          }
                        },
                  child: const Text("Submit"),
                )
              ],
            );
          });
        });
  }

  void _triggerAction(String action, IconData icon, Color color) {
    if (widget.onSwipe == null) {
      Navigator.pop(context); // Fallback close
      return;
    }
    // DO NOT double-pop here. The callback passed from WisslerScreen handles closing this route with a result.
    widget.onSwipe!(action);
  }

  Widget _buildBottomActionBar(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(40, 5, 40, 10), // Lower buttons
      decoration: BoxDecoration(
        color: Colors.amber.withOpacity(0.1), // Light Orange
        boxShadow: [
          BoxShadow(
              color: Colors.black.withOpacity(0.05),
              blurRadius: 20,
              offset: const Offset(0, -5))
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          // Dislike (Large X)
          _ActionButton(
            icon: Icons.close,
            color: Colors.redAccent,
            size: 60,
            onTap: () => _triggerAction('left', Icons.close, Colors.redAccent),
          ),
          // Super Like (Small Star)
          _ActionButton(
            icon: Icons.star,
            color: Colors.blueAccent,
            size: 45,
            onTap: () => _triggerAction('up', Icons.star, Colors.blueAccent),
          ),
          // Chat / Boost (Small Bubble)
          _ActionButton(
            icon: Icons.chat_bubble,
            color: Theme.of(context).primaryColor,
            size: 45,
            onTap: () {
              final displayName = _fullProfile?.displayName ??
                  widget.profile?.displayName ??
                  'User';
              final avatarUrl = _photos.isNotEmpty
                  ? _photos.first['url']
                  : (widget.profile?.avatarUrl ?? '');
              Navigator.push(
                  context,
                  MaterialPageRoute(
                      builder: (_) => ChatDetailScreen(
                            userId: widget.profile?.userId ??
                                _fullProfile!.userId.toString(),
                            userName: displayName,
                            avatarUrl: avatarUrl,
                          )));
            },
          ),
          // Like (Large Heart)
          _ActionButton(
            icon: Icons.favorite,
            color: Colors.green,
            size: 60,
            onTap: () => _triggerAction('right', Icons.favorite, Colors.green),
          ),
        ],
      ),
    );
  }

  Widget _buildAttributeChip(IconData icon, String label) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.grey[300]!),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: Colors.grey[600]),
          const SizedBox(width: 6),
          Text(label,
              style: TextStyle(
                  color: Colors.grey[800], fontWeight: FontWeight.w500)),
        ],
      ),
    );
  }

  String _calculateAge(DateTime dob) {
    final now = DateTime.now();
    int age = now.year - dob.year;
    if (now.month < dob.month ||
        (now.month == dob.month && now.day < dob.day)) {
      age--;
    }
    return age.toString();
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final Color color;
  final VoidCallback onTap;
  final double size;

  const _ActionButton(
      {required this.icon,
      required this.color,
      required this.onTap,
      this.size = 50});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
            boxShadow: [
              BoxShadow(
                  color: Colors.black.withOpacity(0.1),
                  blurRadius: 10,
                  offset: const Offset(0, 5))
            ]),
        child: Icon(icon, color: color, size: size * 0.5),
      ),
    );
  }
}
