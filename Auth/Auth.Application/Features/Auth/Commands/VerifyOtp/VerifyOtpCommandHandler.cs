using MediatR;
using Auth.Domain.Entities;
using Shared.Kernel;
using Auth.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Auth.DTOs;
using Auth.Application.Common.Interfaces;

namespace Auth.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, global::Auth.Application.Features.Auth.DTOs.AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserOtp> _otpRepository;
    private readonly ITokenService _tokenService;
    private readonly Microsoft.Extensions.Logging.ILogger<VerifyOtpCommandHandler> _logger;

    public VerifyOtpCommandHandler(
        IUserRepository userRepository, 
        IRepository<UserOtp> otpRepository,
        ITokenService tokenService,
        Microsoft.Extensions.Logging.ILogger<VerifyOtpCommandHandler> logger)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<global::Auth.Application.Features.Auth.DTOs.AuthResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch User (Lightweight) to find ID
        var user = await _userRepository.GetByEmailOrPhoneAsync(request.Email);
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        // 2. Fetch Active OTP
        var otps = await _otpRepository.ListAsync(o => 
            o.UserId == user.Id && 
            o.Type == request.Type && 
            !o.IsUsed && 
            o.ExpiresAt > DateTime.UtcNow
        );
        
        var validOtp = otps.OrderByDescending(o => o.ExpiresAt).FirstOrDefault();

        if (validOtp == null) throw new UnauthorizedAccessException("Invalid or expired OTP.");

        if (validOtp.Code != request.Code)
        {
            validOtp.IncrementAttempts();
            await _otpRepository.UpdateAsync(validOtp);

            if (validOtp.Attempts >= 5)
            {
                user.BlockOtp(DateTime.UtcNow.AddMinutes(10));
                validOtp.MarkAsUsed();
                await _otpRepository.UpdateAsync(validOtp);
                await _userRepository.UpdateAsync(user);
            }
            throw new UnauthorizedAccessException("Invalid OTP.");
        }

        // 3. Success - Mark OTP used
        validOtp.MarkAsUsed();
        await _otpRepository.UpdateAsync(validOtp);

        // 4. Fetch Full User (with Memberships) for Status Updates
        var fullUser = await _userRepository.GetUserWithRolesAsync(user.Id);
        if (fullUser == null) fullUser = user; // Fallback, though unlikely

        if (request.Type == VerificationType.Email) fullUser.VerifyEmail();
        if (request.Type == VerificationType.Phone) fullUser.VerifyPhone();

        // App-Level Status updates (like PendingApproval) are now handled by Users.API.
        // We evaluate Global Identity Status based on overall verification progress.
        if (fullUser.IsEmailVerified && fullUser.IsPhoneVerified)
            fullUser.SetStatus(GlobalUserStatus.Active);
        else if (!fullUser.IsEmailVerified && !fullUser.IsPhoneVerified) 
            fullUser.SetStatus(GlobalUserStatus.PendingAccountVerification);
        else if (!fullUser.IsEmailVerified)
            fullUser.SetStatus(GlobalUserStatus.PendingEmailVerification);
        else if (!fullUser.IsPhoneVerified)
            fullUser.SetStatus(GlobalUserStatus.PendingMobileVerification);

        _logger.LogInformation("VerifyOTP: Updated User {UserId} GlobalStatus={GStatus}", fullUser.Id, fullUser.Status);

        await _userRepository.UpdateAsync(fullUser);

        // 6. Generate Tokens (Auto-Login)
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Create Session *before* Access Token so we can embed the Session ID (sid)
        var session = new UserSession(
            userId: user.Id,
            appId: null, 
            refreshToken: refreshToken,
            expiresAt: DateTime.UtcNow.AddDays(7), // Refresh Token Expiry
            ipAddress: "0.0.0.0", // TODO: Pass from Controller
            userAgent: "OTP/Unknown" // TODO: Pass from Controller
        );

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(fullUser, null, session.Id);

        try
        {
            await _userRepository.AddSessionAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist User Session.");
        }
        
        return new global::Auth.Application.Features.Auth.DTOs.AuthResponse(accessToken, refreshToken, expiresIn);
    }
}
