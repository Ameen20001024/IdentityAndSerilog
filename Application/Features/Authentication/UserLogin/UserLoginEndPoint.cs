using Azure;
using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.UserLogin
{
    public class UserLoginEndPoint
    {
        public static void MapUserLoginEndpoint(IEndpointRouteBuilder app)

        {
            app.MapPost
            (
                "/api/auth/login",
                async (UserLoginCommand request, ISender sender, HttpResponse response) =>
                {

                    var result = await sender.Send(request);

                    if (!result.IsSucceeded)
                    {
                        return Results.BadRequest(result);
                    }

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddHours(2)
                    };

                    response.Cookies.Append("access_token", result.AccessToken, cookieOptions);

                    response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);

                    return Results.Ok(result);
                }
            );
        
        }
    }
}
