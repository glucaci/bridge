using Bridge;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static BusBridgeBuilder AddBridge(
        this IServiceCollection services)
    {
        services.AddSingleton<ISerializer, DefaultSerializer>();
        
        return new BusBridgeBuilder(services);
    }
}