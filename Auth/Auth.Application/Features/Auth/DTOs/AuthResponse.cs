namespace Auth.Application.Features.Auth.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);
