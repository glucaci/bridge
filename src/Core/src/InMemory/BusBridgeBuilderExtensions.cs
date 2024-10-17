using Bridge;
using Bridge.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class BusBridgeBuilderExtensions
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
        builder.Services.AddSingleton<IBrokerMessageBus>(sp => 
            sp.GetRequiredService<IInMemoryMessageBus>());
        builder.Services.TryAddSingleton<IMessageBus>(sp => 
            sp.GetRequiredService<IBrokerMessageBus>());
    }
}