import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../repositories/app_repository.dart';
import 'auth_provider.dart';

final appRepositoryProvider = Provider<AppRepository>((ref) {
  return AppRepository(ref.watch(authClientProvider));
});

// We can also add a FutureProvider for the AppConfig if we want to cache it per session
// but for now let's just expose the repo.
