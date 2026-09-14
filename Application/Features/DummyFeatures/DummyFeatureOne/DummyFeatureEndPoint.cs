using IdentityAndSerilog.Common;
using MediatR;

namespace IdentityAndSerilog.Application.Features.DummyFeatures.DummyFeatureOne
{
    public class DummyFeatureEndPoint
    {
        public static void MapDummyFeatureEndPoint(IEndpointRouteBuilder app)
        {
            app.MapGet
                (
                    "/api/dummyfeature",
                    async (ISender sender, CancellationToken cancellationToken) => 
                    {
                        var response = await sender.Send(new DummyFeatureQuery(), cancellationToken);

                        return Results.Ok(response);
                    }
                ).RequireAuthorization(Policies.AdminOnly);
        }
    }
}
