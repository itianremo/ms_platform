using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Auth.DTOs;
using global::Auth.Domain.Services;
using MassTransit;
using Shared.Messaging.Events;
using global::Auth.Domain.Exceptions;
using System.Net.Http.Json;

namespace Auth.Application.Features.Auth.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginUserCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<LoginUserCommandHandler> logger,
        IPublishEndpoint publishEndpoint,
        IHttpClientFactory httpClientFactory)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _httpClientFactory = httpClientFactory;
    }

    public class LocalAuthProfileDto
    {
        public Guid RoleId { get; set; }
        public int Status { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
    }

    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailOrPhoneAsync(request.Email);
        
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for identifier '{Email}'", request.Email);
            await Task.Delay(100, cancellationToken); 
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // 1. Brute Force Check
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
             _logger.LogWarning("Login blocked: User {UserId} is locked out until {LockoutEnd}", user.Id, user.LockoutEnd);
             throw new AccountLockedException(user.LockoutEnd.Value);
        }

        // 2. Password Check
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user '{UserId}'", user.Id);
            
            user.IncrementAccessFailedCount();
            
            // Check Lockout Threshold
            if (user.AccessFailedCount >= 5)
            {
                var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.Lockout(lockoutEnd);
                _logger.LogWarning("User {UserId} locked out due to excessive failed attempts.", user.Id);
                
                await _publishEndpoint.Publish(new UserLockedOutEvent(
                    user.Id, 
                    user.Email, 
                    lockoutEnd, 
                    request.IpAddress ?? "Unknown"
                ), cancellationToken);
            }

            await _userRepository.UpdateAsync(user);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Reset counters on success
        if (user.AccessFailedCount > 0 || user.LockoutEnd.HasValue)
        {
            user.ResetAccessFailedCount();
        }

        user.RecordLogin(request.AppId);

        // 3. Fetch App Profile (Role, Status, Subscription) from Users.API
        Role? appRole = null;
        Role? superAdminRole = null;
        bool suppressRoles = false;
        var appRequirements = new List<AppRequirement>();

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://ms_users:8080/"); // Internal container DNS
        
        if (request.AppId.HasValue)
        {
            var appReq = await _userRepository.GetAppVerificationConfigAsync(request.AppId.Value);
            
            try
            {
                var response = await client.GetAsync($"api/Internal/auth-profile?userId={user.Id}&appId={request.AppId.Value}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<LocalAuthProfileDto>(cancellationToken: cancellationToken);
                    if (profile != null)
                    {
                        if (appReq != null)
                        {
                            appReq.MembershipStatus = profile.Status;
                            appRequirements.Add(appReq);
                        }

                        appRole = await _userRepository.GetRoleByIdAsync(profile.RoleId);

                        if (profile.SubscriptionExpiry.HasValue && profile.SubscriptionExpiry.Value < DateTime.UtcNow)
                        {
                            suppressRoles = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Users.API AuthProfile for User {UserId} in App {AppId}", user.Id, request.AppId.Value);
            }

            // Check if SuperAdmin in Global App
            var globalAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            if (request.AppId.Value != globalAppId)
            {
                try
                {
                     var globalResponse = await client.GetAsync($"api/Internal/auth-profile?userId={user.Id}&appId={globalAppId}", cancellationToken);
                     if (globalResponse.IsSuccessStatusCode)
                     {
                         var globalProfile = await globalResponse.Content.ReadFromJsonAsync<LocalAuthProfileDto>(cancellationToken: cancellationToken);
                         if (globalProfile != null && globalProfile.Status == 0) // Active
                         {
                             var globalRole = await _userRepository.GetRoleByIdAsync(globalProfile.RoleId);
                             if (globalRole?.Name == "SuperAdmin") superAdminRole = globalRole;
                         }
                     }
                }
                catch { }
            }
        }

        // 3.5. Policy Check
        var policy = new UserLoginPolicy();
        var policyResult = policy.Evaluate(user, request.AppId, appRequirements);
        
        if (policyResult.SuppressRoles) suppressRoles = true;

        // 4. Session Limit Check (1 per App)
        if (request.AppId.HasValue)
        {
            var existingSessions = user.Sessions?
                .Where(s => s.AppId == request.AppId.Value && s.ExpiresAt > DateTime.UtcNow && !s.IsRevoked)
                .ToList() ?? new List<UserSession>();

            if (existingSessions.Any())
            {
                 _logger.LogInformation("Enforcing Single Session Limit for User {UserId} on App {AppId}. Revoking {Count} sessions.", user.Id, request.AppId.Value, existingSessions.Count);
                 foreach (var s in existingSessions)
                 {
                     s.Revoke();
                 }
            }
        }
        
        var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);

        // 5. Generate Tokens
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        var session = new UserSession(
            userId: user.Id,
            appId: request.AppId,
            refreshToken: refreshToken,
            expiresAt: DateTime.UtcNow.AddDays(7),
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent
        );

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(
            userWithRoles ?? user, 
            request.AppId, 
            session.Id, 
            suppressRoles: suppressRoles,
            appRole: appRole,
            superAdminRole: superAdminRole
        );

        // Persist
        try
        {
            await _userRepository.AddSessionAsync(session);
            await _userRepository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist session/user updates.");
            throw; 
        }

        return new AuthResponse(accessToken, refreshToken, expiresIn);
    }
}
