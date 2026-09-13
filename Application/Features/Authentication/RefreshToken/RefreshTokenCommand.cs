using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<RefreshTokenResponse>;