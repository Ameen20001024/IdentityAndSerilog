using Azure;
using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.RefreshToken
{
    public class RefreshTokenEndPoint
    {
        public static void MapRefreshTokenEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/refresh-token",
                async (
                    HttpContext httpContext, ISender sender, HttpResponse response) =>
                {
                    var accessToken = httpContext.Request.Cookies["access_token"];

                    var refreshToken = httpContext.Request.Cookies["refresh_token"];

                    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    {
                        return Results.Unauthorized();
                    }

                    var command = new RefreshTokenCommand(accessToken, refreshToken);

                    var result = await sender.Send(command);

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
