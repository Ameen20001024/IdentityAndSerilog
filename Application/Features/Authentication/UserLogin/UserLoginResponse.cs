namespace IdentityAndSerilog.Application.Features.Authentication.UserLogin
{
    public record UserLoginResponse
        (
            string AccessToken,
            string RefreshToken,
            int ExpiresIn,
            bool IsSucceeded,
            string[] ErrorMessage
        );

}