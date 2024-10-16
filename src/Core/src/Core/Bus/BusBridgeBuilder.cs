using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bridge;

public class BusBridgeBuilder
{
    private readonly List<ConsumerConfigurationBuilder> _consumers = new();

    internal BusBridgeBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal IReadOnlyList<ConsumerConfigurationBuilder> Consumers => _consumers;

    public IServiceCollection Services { get; }

    public BusBridgeBuilder AddConsumer<TConsumer, TMessage>(
        string queueName,
        Action<ConsumerConfiguration>? configure = default)
        where TConsumer : class, IConsumer<TMessage>
    {
        Services.AddSingleton<TConsumer>();
        
        var builder = new ConsumerConfigurationBuilder(queueName, sp =>
        {
            var serializer = sp.GetRequiredService<ISerializer>();
            
            var consumerConfiguration = ConsumerConfiguration
                .Create<TConsumer, TMessage>(queueName, serializer);

            configure?.Invoke(consumerConfiguration);

            return consumerConfiguration;
        });

        _consumers.Add(builder);

        return this;
    }
    
    public BusBridgeBuilder AddOutbox(Action<BusBridgeOutboxBuilder> configure)
    {
        Services.AddHostedService<OutboxService>();
        Services.AddSingleton<IOutboxProcessor, OutboxProcessor>();

        var builder = new BusBridgeOutboxBuilder(Services);
        configure(builder);

        return this;
    }
}
