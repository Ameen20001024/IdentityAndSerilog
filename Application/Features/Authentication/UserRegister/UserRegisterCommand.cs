using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.UserRegister
{
    public record UserRegisterCommand
    (
        string Username,
        string Email,
        string Password
    ) : IRequest<UserRegisterResponse>;
}
