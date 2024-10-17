using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bridge.InMemory;

internal class InMemoryProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfiguration _consumerConfiguration;
    private readonly TimeProvider _timeProvider;
    private readonly IInMemoryMessageBus _messageBus;

    public InMemoryProcessor(
        IServiceProvider serviceProvider,
        ConsumerConfiguration consumerConfiguration, 
        TimeProvider timeProvider)
    {
        _serviceProvider = serviceProvider;
        _consumerConfiguration = consumerConfiguration;
        _timeProvider = timeProvider;
        _messageBus = _serviceProvider.GetRequiredService<IInMemoryMessageBus>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var queue = _messageBus.GetQueue(_consumerConfiguration.QueueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (await queue.WaitForMessages(stoppingToken))
                {
                    while (queue.TryPeek(out var inMemoryMessage))
                    {
                        var now = _timeProvider.GetUtcNow();

                        if (now >= inMemoryMessage.EnqueueTime)
                        {
                            await _consumerConfiguration
                                .HandleMessage(_serviceProvider, inMemoryMessage.CloudEvent, stoppingToken);
                            
                            _ = await queue.Dequeue(stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}