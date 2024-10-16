using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Memory;

namespace Bridge.Bus.AzureServiceBus;

internal class AzureServiceBusSender : IBrokerMessageBus
{
    /// <summary>
    /// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample11_CloudEvents.md
    /// </summary>
    private const string CloudEventsContentType = "application/cloudevents+json";

    private readonly ServiceBusClient _client;
    private readonly IMemoryCache _queueCache;

    public AzureServiceBusSender(
        ServiceBusClient client,
        IMemoryCache queueCache)
    {
        _client = client;
        _queueCache = queueCache;
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
        ServiceBusSender sender = CreateQueueSender(queue);

        CloudEvent cloudEvent = new CloudEvent(
            _client.Identifier, typeof(TMessage).Name, message);
        
        ServiceBusMessage serviceBusMessage =
            new ServiceBusMessage(new BinaryData(cloudEvent))
            {
                ContentType = CloudEventsContentType
            };

        if (scheduledEnqueueTime.HasValue)
        {
            serviceBusMessage.ScheduledEnqueueTime = scheduledEnqueueTime.Value;
        }

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    private ServiceBusSender CreateQueueSender(string queue)
    {
        return _queueCache
            .GetOrCreate(queue, entry => CreateSender(entry, queue))!;
    }

    private ServiceBusSender CreateSender(ICacheEntry entry, string queue)
    {
        entry.Size = 1;
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
        entry.SetPriority(CacheItemPriority.NeverRemove);
        entry.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            if (value is ServiceBusSender sender)
            {
                _ = sender.DisposeAsync();
            }
        });

        return _client.CreateSender(queue);
    }
}
