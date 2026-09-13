using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.UserLogin
{
    public record UserLoginCommand(
        string Email,
        string Password
        ) : IRequest<UserLoginResponse>;
    
}
