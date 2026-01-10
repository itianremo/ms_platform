using Auth.Application.Common.Interfaces;

namespace Auth.Infrastructure.Services;

public class OtpService : IOtpService
{
    // In a real app, use Redis to store OTPs with expiration.
    // Use an Email/SMS provider (e.g. Twilio, SendGrid) to send codes.
    // For now, we mock it.
    private static readonly Dictionary<string, string> _mockStore = new();

    public Task SendOtpAsync(string email, string purpose)
    {
        var otp = "123456"; // Fixed OTP for testing
        _mockStore[email + purpose] = otp;
        
        Console.WriteLine($"[OTP SERVICE] Generated OTP for {email} ({purpose}): {otp}");
        
        return Task.CompletedTask;
    }

    public Task<bool> VerifyOtpAsync(string email, string purpose, string code)
    {
        if (_mockStore.TryGetValue(email + purpose, out var storedOtp))
        {
            if (storedOtp == code)
            {
                _mockStore.Remove(email + purpose);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
