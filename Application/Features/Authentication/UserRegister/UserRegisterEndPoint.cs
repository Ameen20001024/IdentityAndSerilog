using MediatR;

namespace IdentityAndSerilog.Application.Features.Authentication.UserRegister
{
    public class UserRegisterEndPoint
    {
        public static void MapUserRegisterEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPost
                (
                    "/api/auth/register",
                    async (UserRegisterCommand request, ISender sender) =>
                    {
                        var result = await sender.Send(request);

                        if (!result.success)
                        {
                            return Results.BadRequest(result);
                        }

                        return Results.Ok(result);
                    }
                );
        }
    }
}
