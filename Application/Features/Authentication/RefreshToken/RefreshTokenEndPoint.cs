using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.RefreshToken
{
    public class RefreshTokenEndPoint
    {
        public static void MapRefreshTokenEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/refresh-token",
                async (
                    HttpContext httpContext, ISender sender) =>
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

                    return Results.Ok(result);
                }
            );
        }
    }
}
