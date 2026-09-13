namespace IdentityAndSerilog.Application.Features.Authentication.UserRegister
{
    public record UserRegisterResponse
        (
            int UserId,
            string Username,
            string Email,bool success,
            string[] errors
        );
    
}
