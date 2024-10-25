using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using CloudNative.CloudEvents;

namespace Bridge.Bus.InMemory;

internal class InMemoryMessageBus : IInMemoryMessageBus
{
    private readonly ConcurrentDictionary<string, InMemoryQueue<InMemoryMessage>> _queues = new();
    private readonly TimeProvider _timeProvider;

    public InMemoryMessageBus(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public ValueTask Send<TMessage>(
        TMessage message,
        string queue,
        CancellationToken cancellationToken)
    {
        return SendMessage(message, queue, cancellationToken);
    }

    public ValueTask Schedule<TMessage>(
        TMessage message,
        string queue,
        DateTimeOffset enqueueTime,
        CancellationToken cancellationToken)
    {
        return SendMessage(message, queue, cancellationToken, enqueueTime);
    }

    private async ValueTask SendMessage<TMessage>(
        TMessage message,
        string queue,
        CancellationToken cancellationToken,
        DateTimeOffset? scheduledEnqueueTime = default)
    {
        var queueInstance = GetQueue(queue);

        DateTimeOffset enqueueTime = _timeProvider.GetUtcNow();

        if (scheduledEnqueueTime.HasValue)
        {
            enqueueTime = scheduledEnqueueTime.Value;
        }
        
        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("/localhost"),
            Type = typeof(TMessage).Name,
            Data = message,
            DataContentType = "application/json",
            Time = DateTimeOffset.Now
        };

        var inMemoryMessage = new InMemoryMessage(enqueueTime, cloudEvent);

        await queueInstance.Enqueue(inMemoryMessage, cancellationToken);
    }

    public InMemoryQueue<InMemoryMessage> GetQueue(string queue)
    {
        return _queues.GetOrAdd(queue, _ =>
            new InMemoryQueue<InMemoryMessage>(Channel.CreateBounded<InMemoryMessage>(
                new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait })));
    }
}