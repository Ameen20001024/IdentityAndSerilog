using IdentityAndSerilog.Common;
using IdentityAndSerilog.Data;
using IdentityAndSerilog.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndSerilog.Application.Features.Authentication.UserLogin
{
    public class UserLoginHandler : IRequestHandler<UserLoginCommand, UserLoginResponse>
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtHelper _jwtHelper;
        private readonly AppDbContext _context;
        private readonly JwtOptions _jwtOptions;

        public UserLoginHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtHelper jwtHelper,
            AppDbContext context,
            JwtOptions jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
            _context = context;
            _jwtOptions = jwtOptions;
        }

        public async Task<UserLoginResponse> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            // Find user
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return new UserLoginResponse(
                    string.Empty,
                    string.Empty,
                    0,
                    false,
                    ["Invalid email or password."]);
            }

            // Verify password using ASP.NET Core Identity
            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                return new UserLoginResponse(
                    string.Empty,
                    string.Empty,
                    0,
                    false,
                    ["Invalid email or password."]);
            }

            // Generate access token
            var accessToken = _jwtHelper.GenerateAccessToken(user);

            // Generate refresh token
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Store only the hash in database
            var refreshTokenEntity = new Domain.Models.RefreshTokens
            {
                TokenHash = HashToken(refreshToken),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync(cancellationToken);

            return new UserLoginResponse(
               
                accessToken,
                refreshToken,
                _jwtOptions.RefreshTokenExpirationDays,
                true,
                []);
        }

        private static string HashToken(string token)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(bytes);
        }
    }
}
