using Bridge.Bus;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static BusBridgeBuilder AddBridgeBus(
        this IServiceCollection services)
    {
        services.AddSingleton<ISerializer, DefaultSerializer>();
        
        return new BusBridgeBuilder(services);
    }
}