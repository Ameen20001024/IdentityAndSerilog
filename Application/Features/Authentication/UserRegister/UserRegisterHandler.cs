using IdentityAndSerilog.Application.Features.Authentication.UserLogin;
using IdentityAndSerilog.Domain.Models;
using IdentityAndSerilog.Logging;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace IdentityAndSerilog.Application.Features.Authentication.UserRegister
{
    public class UserRegisterHandler : IRequestHandler<UserRegisterCommand, UserRegisterResponse>
    {

        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserLoginHandler> _logger;

        public UserRegisterHandler(UserManager<User> userManager, ILogger<UserLoginHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }


        

        public async Task<UserRegisterResponse> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                _logger
                .LogError(new EventId(SecurityEventIds.ForbiddenAccess,
                    nameof(SecurityEventIds.ForbiddenAccess)),
                    "User email already exists. Invalid email or password.");


                throw new InvalidOperationException("User with this email already exists.");
            }

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                _logger
                .LogError(new EventId(SecurityEventIds.RegisterFailed,
                    nameof(SecurityEventIds.RegisterFailed)),
                    "User register failed.");

                return new UserRegisterResponse(0, string.Empty, string.Empty, false, result.Errors.Select(x => x.Description).ToArray());
            }

            _logger
                .LogInformation(new EventId(SecurityEventIds.RegisterSucceeded,
                    nameof(SecurityEventIds.RegisterSucceeded)),
                    "User registered successfully.");


            var response = new UserRegisterResponse(
                user.Id,
                user.UserName,
                user.Email, result.Succeeded, []);


            return response;
        }
    }
}
