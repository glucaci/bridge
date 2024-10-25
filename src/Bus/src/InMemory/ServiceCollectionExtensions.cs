using Bridge.Bus;
using Bridge.Bus.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void UsingInMemory(
        this BusBridgeBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        
        builder.Services.TryAddSingleton<TimeProvider>(_ => TimeProvider.System);

        foreach (var consumer in builder.Consumers)
        {
            builder.Services.AddHostedService(sp =>
            {
                var consumerConfiguration = consumer.Create(sp);
                var timeProvider = sp.GetRequiredService<TimeProvider>();
                return new InMemoryProcessor(sp, consumerConfiguration, timeProvider);
            });
        }

        builder.Services.AddSingleton<IInMemoryMessageBus, InMemoryMessageBus>();
        builder.Services.TryAddSingleton<IMessageBus>(sp => sp.GetRequiredService<IInMemoryMessageBus>());
    }
}