namespace IdentityAndSerilog.Application.Features.Authentication.RefreshToken;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int RefreshTokenExpirationDays,
    bool IsSucceeded,
    string[] Errors);