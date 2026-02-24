using MediatR;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Application.Common.Interfaces;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IPublishEndpoint publishEndpoint,
        Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check for Existing User (Email or Phone)
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser == null && !string.IsNullOrEmpty(request.Phone))
        {
            existingUser = await _userRepository.GetByEmailOrPhoneAsync(request.Phone);
        }

        if (existingUser != null)
        {
            // --- Existing User Logic ---

            // A. Deleted Account
            if (existingUser.Status == GlobalUserStatus.SoftDeleted)
            {
                // Throwing specific exception for UI to handle "Restore" flow
                throw new global::Auth.Domain.Exceptions.UserSoftDeletedException("Account is deleted. Restoration is available.");
            }

            // Reload User with Roles/Memberships to ensure we have the latest state
            existingUser = await _userRepository.GetUserWithRolesAsync(existingUser.Id) ?? existingUser;

            // B. Existing User Registration
            // Since Auth.API no longer has visibility into memberships, we publish the event to Users.API.
            // Users.API's UserRegisteredConsumer is idempotent and will ignore profile creation if the user is already in the app.

            // C. Exists in OTHER App -> Register in New App
            if (request.AppId.HasValue)
            {
                var roleId = await ResolveRoleIdAsync(request.AppId.Value);
                await RegisterExistingUserInNewApp(existingUser, request.AppId.Value, request.VerificationType, request.RequiresAdminApproval);
                
                await PublishUserRegisteredEvent(existingUser, request.AppId.Value, request.Password, roleId, cancellationToken);
                
                return existingUser.Id;
            }
            
            // Fallback if no AppId provided but user exists
            throw new InvalidOperationException("User account already exists.");
        }

        // --- New User Logic ---
        
        var passwordHash = _passwordHasher.Hash(request.Password);
        var newUser = new User(request.Email, request.Phone, passwordHash);

        // Determine Initial Status
        ResolveNewUserStatus(newUser, request.VerificationType, request.RequiresAdminApproval);

        // Add Membership if App provided
        Guid? assignedRoleId = null;
        if (request.AppId.HasValue)
        {
            assignedRoleId = await ResolveRoleIdAsync(request.AppId.Value);
        }

        // Save
        await _userRepository.AddAsync(newUser);

        // Publish Event
        if (request.AppId.HasValue)
        {
            await PublishUserRegisteredEvent(newUser, request.AppId.Value, request.Password, assignedRoleId, cancellationToken);
        }

        return newUser.Id;
    }

    private void ResolveNewUserStatus(User user, VerificationType verificationType, bool requiresAdminApproval)
    {
        // Prioritize Verification -> Admin Approval -> ProfileIncomplete
        
        if (verificationType != VerificationType.None)
        {
            switch (verificationType)
            {
                case VerificationType.Email:
                    user.SetStatus(GlobalUserStatus.PendingEmailVerification);
                    break;
                case VerificationType.Phone:
                    user.SetStatus(GlobalUserStatus.PendingMobileVerification);
                    break;
                case VerificationType.Both:
                    user.SetStatus(GlobalUserStatus.PendingAccountVerification);
                    break;
            }
        }
        else if (requiresAdminApproval)
        {
             user.SetStatus(GlobalUserStatus.PendingAdminApproval);
        }
        else
        {
            // If no immediate verification or admin approval needed, set to ProfileIncomplete
            // Usage: User can login but might be blocked until Profile is filled.
            user.SetStatus(GlobalUserStatus.ProfileIncomplete);
        }
    }

    private async Task RegisterExistingUserInNewApp(User user, Guid appId, VerificationType verificationType, bool requiresAdminApproval)
    {
        // 1. Memberships are now natively handled by Users.API when the UserRegisteredEvent is consumed.

        // 2. Check Verification Requirements vs Current Status
        // If current user is missing verified fields required by this new app, degrade global status?
        // Risky for existing apps. Usually, we just ensure the verification flow is triggered.
        // User.cs has methods UpdateEmail/Phone which reset verification.
        // Here we just check completeness.

        bool emailNeeded = (verificationType == VerificationType.Email || verificationType == VerificationType.Both);
        bool phoneNeeded = (verificationType == VerificationType.Phone || verificationType == VerificationType.Both);

        if (emailNeeded && !user.IsEmailVerified)
        {
            // If strictly required and not verified, we might need to set status to ensure they verify.
            // But we shouldn't block them if they are Active elsewhere? 
            // The prompt says "Register him... consider if ... verifications are required".
            // We set the Global Status to pending if they are missing a credential verification.
            if (user.Status == GlobalUserStatus.Active || user.Status == GlobalUserStatus.ProfileIncomplete)
            {
                 user.SetStatus(GlobalUserStatus.PendingEmailVerification);
            }
        }
        else if (phoneNeeded && !user.IsPhoneVerified)
        {
             if (user.Status == GlobalUserStatus.Active || user.Status == GlobalUserStatus.ProfileIncomplete)
             {
                  user.SetStatus(GlobalUserStatus.PendingMobileVerification);
             }
        }

        // We do NOT force ProfileIncomplete on Existing users to avoid disrupting their other apps.
        // The App-Level profile check should be enforced at Login or Usage time for that specific App.
        
        await _userRepository.UpdateAsync(user);
    }

    private async Task<Guid> ResolveRoleIdAsync(Guid appId)
    {
        var userRole = await _userRepository.GetRoleByNameAsync(appId, "Visitor") 
                       ?? await _userRepository.GetRoleByNameAsync(appId, "User"); // Fallback

        if (userRole == null)
        {
            // Create default role if missing
            userRole = new Role(appId, "Visitor");
            await _userRepository.AddRoleAsync(userRole);
        }

        return userRole.Id;
    }

    private async Task PublishUserRegisteredEvent(User user, Guid appId, string password, Guid? roleId, CancellationToken cancellationToken)
    {
        try
        {
            await _publishEndpoint.Publish(new UserRegisteredEvent(
                user.Id, 
                appId, 
                user.Email, 
                user.Phone,
                user.Email,
                password,
                roleId
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for User {UserId}", user.Id);
        }
    }
}
