using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndSerilog.Application.Features.DummyFeatures.DummyFeatureOne
{
    public class DummyFeatureHandler : IRequestHandler<DummyFeatureQuery, DummyFeatureResponse>
    {
        public Task<DummyFeatureResponse> Handle(DummyFeatureQuery request, CancellationToken cancellationToken)
        {
            var message = "This is a dummy endpoint";

            return Task.FromResult( new DummyFeatureResponse(message));
        }
    }
}
