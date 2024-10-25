using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Bridge.Bus.InMemory.Tests;

public class InMemoryBusHost<TConsumer, TMessage> 
    where TConsumer : class, IConsumer<TMessage>
{
    private InMemoryBusHost(
        FakeTimeProvider timeProvider,
        IInMemoryMessageBus messageBus,
        TConsumer consumer,
        string queueName)
    {
        TimeProvider = timeProvider;
        MessageBus = messageBus;
        Consumer = consumer;
        QueueName = queueName;
    }

    public FakeTimeProvider TimeProvider { get; }
    internal IInMemoryMessageBus MessageBus { get; }
    public TConsumer Consumer { get; }
    public string QueueName { get; }

    public Task WaitForConsumer(string queueName)
    {
        var queue = MessageBus.GetQueue(queueName);
        queue.Close();

        return queue.Waiter;
    }

    public static async Task<InMemoryBusHost<TConsumer, TMessage>> Create()
    {
        string queueName = Guid.NewGuid().ToString("N");
        var timeProvider = new FakeTimeProvider();

        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(timeProvider);

        services
            .AddBridgeBus()
            .AddConsumer<TConsumer, TMessage>(queueName)
            .UsingInMemory();
        
        var serviceProvider = services.BuildServiceProvider();
        
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        foreach (var service in hostedServices)
        {
            await service.StartAsync(default);
        }
        
        var messageBus = serviceProvider.GetRequiredService<IInMemoryMessageBus>();
        var consumer = serviceProvider.GetRequiredService<TConsumer>();

        return new InMemoryBusHost<TConsumer, TMessage>(timeProvider, messageBus, consumer, queueName);
    }
}