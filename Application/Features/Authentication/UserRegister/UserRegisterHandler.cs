using IdentityAndSerilog.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace IdentityAndSerilog.Application.Features.Authentication.UserRegister
{
    public class UserRegisterHandler : IRequestHandler<UserRegisterCommand, UserRegisterResponse>
    {

        private readonly UserManager<User> _userManager;

        public UserRegisterHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        

        public async Task<UserRegisterResponse> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
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
                return new UserRegisterResponse(0, string.Empty, string.Empty, false, result.Errors.Select(x => x.Description).ToArray());
            }
            

            var response = new UserRegisterResponse(
                user.Id,
                user.UserName,
                user.Email, result.Succeeded, []);


            return response;
        }
    }
}
