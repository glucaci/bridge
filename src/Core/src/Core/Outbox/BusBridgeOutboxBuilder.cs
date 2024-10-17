using Microsoft.Extensions.DependencyInjection;

namespace Bridge;

public class BusBridgeOutboxBuilder
{
    internal BusBridgeOutboxBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }
}