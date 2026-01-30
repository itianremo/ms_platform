using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Auth.DTOs;
using DomainUser = global::Auth.Domain.Entities.User;

namespace Auth.Application.Features.Auth.Commands.LoginUser;


public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Features.Auth.DTOs.AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly Microsoft.Extensions.Logging.ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        Microsoft.Extensions.Logging.ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Features.Auth.DTOs.AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailOrPhoneAsync(request.Email);
        DomainUser? userWithRoles = null;
        
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for identifier '{Email}'", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user '{Email}'", request.Email);
             throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.Status == Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            throw new Common.Exceptions.UserSoftDeletedException("Account is soft-deleted. Reactivation required.");
        }

        if (user.Status == Domain.Entities.GlobalUserStatus.Banned)
        {
            throw new Common.Exceptions.AccountBannedException();
        }

        if (request.AppId.HasValue)
        {
            // Verify App Exists and is Active
            bool isAppActive = await _userRepository.IsAppActiveAsync(request.AppId.Value);
            if (!isAppActive)
            {
                _logger.LogWarning("Login failed: App {AppId} is not active.", request.AppId.Value);
                throw new UnauthorizedAccessException("Application is not active.");
            }

            // Verify User Membership in this App
            // We need to load memberships.
            userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);
            if (userWithRoles == null || !userWithRoles.Memberships.Any(m => m.AppId == request.AppId.Value))
            {
                 _logger.LogWarning("Login failed: User {UserId} is not a member of App {AppId}", user.Id, request.AppId.Value);
                 // Prompt asks to return "not exist" error
                 throw new UnauthorizedAccessException("User not registered in this application.");
            }
        }

        // Dynamic Verification Enforcement (Upgrade & Downgrade)
        // 1. Fetch Requirements
        _logger.LogInformation("Login Check: Fetching requirements for User {UserId} and App {AppId}", user.Id, request.AppId);
        
        var requirements = await _userRepository.GetMemberAppRequirementsAsync(user.Id);
        _logger.LogInformation("Login Check: Found {Count} requirements.", requirements.Count);

        // 2. Dynamic Enforcement & Check
        bool enforcementSaved = false;
        
        if (requirements.Any())
        {
            GlobalUserStatus? pendingStatus = null;
            bool identityIncomplete = false;

            // Lazy Load Full User only if we need to update entities
            User? fullUser = null; 

            foreach (var req in requirements)
            {
                // A. Identity Verification Check
                bool isIdentityReady = true;
                if ((req.VerificationType == Shared.Kernel.VerificationType.Email || req.VerificationType == Shared.Kernel.VerificationType.Both) && !user.IsEmailVerified) 
                {
                    isIdentityReady = false;
                    pendingStatus = Domain.Entities.GlobalUserStatus.PendingEmailVerification;
                }
                if ((req.VerificationType == Shared.Kernel.VerificationType.Phone || req.VerificationType == Shared.Kernel.VerificationType.Both) && !user.IsPhoneVerified)
                {
                    isIdentityReady = false; 
                    pendingStatus = Domain.Entities.GlobalUserStatus.PendingMobileVerification;
                }

                if (!isIdentityReady) identityIncomplete = true;

                // B. Admin Approval Enforcement (Per App)
                // If App ID matches Request (or we want to enforce generally, but we only block the requested app usually)
                // We update the data for ALL apps, but only block for the requested one.
                
                var currentMembershipStatus = (AppUserStatus)req.MembershipStatus;
                var targetMembershipStatus = currentMembershipStatus; // Default to current

                if (isIdentityReady)
                {
                    // If Identity is ready, check Admin Approval
                    if (req.RequiresAdminApproval)
                    {
                        if (currentMembershipStatus != AppUserStatus.Active)
                        {
                            // Enforce Pending if not explicitly Active
                            targetMembershipStatus = AppUserStatus.PendingApproval;
                        }
                        // If already Active, we leave it Active (Grandfathered/Approved)
                    }
                    else
                    {
                        // If Approval NOT required, ensure Active (if not banned)
                        if (currentMembershipStatus == AppUserStatus.PendingApproval)
                        {
                            targetMembershipStatus = AppUserStatus.Active;
                        }
                    }
                }

                // Apply Membership Update if needed
                if (targetMembershipStatus != currentMembershipStatus && currentMembershipStatus != AppUserStatus.Banned)
                {
                    if (fullUser == null) fullUser = await _userRepository.GetUserWithRolesAsync(user.Id);
                    if (fullUser != null)
                    {
                        fullUser.UpdateMembershipStatus(req.AppId, targetMembershipStatus);
                        enforcementSaved = true;
                        
                        // Update local variable for the BLOCK check below
                        if (req.AppId == request.AppId) currentMembershipStatus = targetMembershipStatus;
                    }
                }

                // C. BLOCK CHECK (Generic for requested App)
                if (request.AppId.HasValue && req.AppId == request.AppId.Value)
                {
                    if (currentMembershipStatus == AppUserStatus.PendingApproval)
                    {
                        // Save changes before throwing if needed?
                        if (enforcementSaved && fullUser != null) await _userRepository.UpdateAsync(fullUser);
                        throw new Common.Exceptions.RequiresAdminApprovalException(); // "Awaiting admin approval"
                    }
                    if (currentMembershipStatus == AppUserStatus.Banned)
                    {
                         throw new Common.Exceptions.AccountBannedException();
                    }
                }
            }

            // Global Status Update (Identity Only)
            var targetGlobalStatus = identityIncomplete 
                ? (pendingStatus ?? Domain.Entities.GlobalUserStatus.PendingEmailVerification) 
                : Domain.Entities.GlobalUserStatus.Active;

            if (user.Status != targetGlobalStatus && user.Status != GlobalUserStatus.Banned && user.Status != GlobalUserStatus.SoftDeleted)
            {
                user.SetStatus(targetGlobalStatus);
                // If we haven't loaded full user, we can save the light user? 
                // Light user is tracked by context? GetByEmail uses context.
                // Yes.
                if (!enforcementSaved) await _userRepository.UpdateAsync(user); 
                else if (fullUser != null) 
                {
                     fullUser.SetStatus(targetGlobalStatus);
                     await _userRepository.UpdateAsync(fullUser);
                }
            }
            else if (enforcementSaved && fullUser != null)
            {
                await _userRepository.UpdateAsync(fullUser);
            }
        }
        else 
        {
             // Fallback Logic (No Requirements found)
             // ... existing fallback ...
        }

        if (user.Status == Domain.Entities.GlobalUserStatus.PendingEmailVerification ||
            user.Status == Domain.Entities.GlobalUserStatus.PendingMobileVerification ||
            user.Status == Domain.Entities.GlobalUserStatus.PendingAccountVerification)
        {
            throw new Common.Exceptions.RequiresVerificationException(user.Status, user.Phone);
        }

        // Removed Global PendingAdminApproval check
        // if (user.Status == Domain.Entities.GlobalUserStatus.PendingAdminApproval) ...

        // Defined at top to avoid scoping issues and double fetch
        // Fetch User with Roles for Token Generation if not already fetched
        if (userWithRoles == null)
        {
             userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);
             if (userWithRoles == null) userWithRoles = user;
        }


        // Generate Refresh Token first
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Create Session *before* Access Token so we can embed the Session ID (sid)
        var session = new UserSession(
            userId: user.Id,
            appId: request.AppId,
            refreshToken: refreshToken,
            expiresAt: DateTime.UtcNow.AddDays(7), // Refresh Token Expiry
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent
        );

        // Generate Access Token with Session ID (sid)
        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(userWithRoles ?? user, request.AppId, session.Id);

        try
        {
            // Direct insert to avoid concurrency issues on User entity
            await _userRepository.AddSessionAsync(session);
        }
        catch (Exception ex)
        {
            // Non-blocking failure: If session persistence fails, we still return the token.
            // However, immediate revocation won't work if session isn't in DB.
            _logger.LogError(ex, "Failed to persist User Session. Immediate revocation will not work for this session.");
        }
        
        return new AuthResponse(accessToken, refreshToken, expiresIn);
    }
}
