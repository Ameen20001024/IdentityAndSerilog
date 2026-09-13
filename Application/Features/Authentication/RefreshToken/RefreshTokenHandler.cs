using IdentityAndSerilog.Common;
using IdentityAndSerilog.Data;
using IdentityAndSerilog.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IdentityAndSerilog.Application.Features.Authentication.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly JwtOptions _jwtOptions;

        public RefreshTokenHandler(
            AppDbContext context,
            JwtHelper jwtHelper,
            JwtOptions jwtOptions)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _jwtOptions = jwtOptions;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var accessTokenHash = HashToken(request.AccessToken);
            var refreshTokenHash = HashToken(request.RefreshToken);


            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.AccessTokenHash == accessTokenHash, cancellationToken);

            if (storedToken is null)
            {
                return FailedResponse("Invalid token.");
            }

            if (!storedToken.IsActive)
            {
                return FailedResponse("Refresh token has expired or been revoked.");
            }

            if (storedToken.TokenHash != refreshTokenHash)
            {
                return FailedResponse("Tokens don't match");
            }

            storedToken.RevokedAt = DateTime.UtcNow;

            var newAccessToken = _jwtHelper.GenerateAccessToken(storedToken.User);

            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshTokens
            {
                TokenHash = HashToken(newRefreshToken),

                AccessTokenHash = HashToken(newAccessToken),

                UserId = storedToken.UserId,

                CreatedAt = DateTime.UtcNow,

                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtOptions.ExpirationMinutes),

                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);

            await _context.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken,
                _jwtOptions.RefreshTokenExpirationDays,
                true,
                []
                );

        }
    

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(bytes);
        }

        private RefreshTokenResponse FailedResponse(string error)
        {
            return new RefreshTokenResponse(
                string.Empty,
                string.Empty,
                0,
                false,
                [error]);
        }

    }

}
