using IdentityAndSerilog.Common;
using IdentityAndSerilog.Data;
using IdentityAndSerilog.Domain.Models;
using IdentityAndSerilog.Logging;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
//using Serilog;

namespace IdentityAndSerilog.Application.Features.Authentication.UserLogin
{
    public class UserLoginHandler : IRequestHandler<UserLoginCommand, UserLoginResponse>
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtHelper _jwtHelper;
        private readonly AppDbContext _context;
        private readonly JwtOptions _jwtOptions;

        private readonly ILogger<UserLoginHandler> _logger;

        public UserLoginHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtHelper jwtHelper,
            AppDbContext context,
            IOptions<JwtOptions> jwtOptions,
            ILogger<UserLoginHandler> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
            _context = context;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<UserLoginResponse> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            // Find user
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger
                .LogError(new EventId(SecurityEventIds.UnknownUserLoginAttempt, nameof(SecurityEventIds.UnknownUserLoginAttempt)), "User not found. Login attempt for unknown user {Email}", request.Email);

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
                if (signInResult.IsLockedOut)
                {
                    _logger.LogWarning(
                        new EventId(SecurityEventIds.AccountLockedOut, nameof(SecurityEventIds.AccountLockedOut)),
                        "Account locked out for {UserId} after repeated failed login attempts", user.Id);
                }
                else
                {
                    _logger
                    .LogError(new EventId(SecurityEventIds.LoginFailed, nameof(SecurityEventIds.LoginFailed)), "User {UserId} login failed. Invalid password.", user.Id);
                }


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
                AccessTokenHash = HashToken(accessToken),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync(cancellationToken);

            _logger
                .LogInformation(new EventId(SecurityEventIds.LoginSucceeded, nameof(SecurityEventIds.LoginSucceeded)), "User {UserId} login succeeded", user.Id);

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
