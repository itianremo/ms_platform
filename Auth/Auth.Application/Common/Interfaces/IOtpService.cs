namespace Auth.Application.Common.Interfaces;

public interface IOtpService
{
    Task SendOtpAsync(string email, string purpose);
    Task<bool> VerifyOtpAsync(string email, string purpose, string code);
}
