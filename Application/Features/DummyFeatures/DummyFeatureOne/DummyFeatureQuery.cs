using MediatR;

namespace IdentityAndSerilog.Application.Features.DummyFeatures.DummyFeatureOne
{
    public record DummyFeatureQuery() : IRequest<DummyFeatureResponse>;
    
}
