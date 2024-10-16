using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Bridge.InMemory.Tests;

public class InMemoryBusHost
{
    private readonly IServiceProvider _serviceProvider;

    private InMemoryBusHost(
        IServiceProvider serviceProvider,
        FakeTimeProvider timeProvider,
        string queueName)
    {
        _serviceProvider = serviceProvider;
        TimeProvider = timeProvider;
        QueueName = queueName;
    }

    public FakeTimeProvider TimeProvider { get; }
    public string QueueName { get; }

    public TMessageBus GetMessageBus<TMessageBus>()
        where TMessageBus : IMessageBus
    {
        return _serviceProvider.GetRequiredService<TMessageBus>();
    }
    
    public TConsumer GetConsumer<TConsumer, TMessage>() 
        where TConsumer : IConsumer<TMessage>
    {
        return _serviceProvider.GetRequiredService<TConsumer>();
    }
    
    public static async Task<InMemoryBusHost> Create(
        Action<string, BusBridgeBuilder> configure)
    {
        string queueName = Guid.NewGuid().ToString("N");
        var timeProvider = new FakeTimeProvider();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(timeProvider);
        
        var bridgeBuilder = services.AddBridge();
        configure.Invoke(queueName, bridgeBuilder);
        bridgeBuilder.UsingInMemory();
        
        var serviceProvider = services.BuildServiceProvider();
        
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        foreach (var service in hostedServices)
        {
            await service.StartAsync(default);
        }

        return new InMemoryBusHost(serviceProvider, timeProvider, queueName);
    }
}