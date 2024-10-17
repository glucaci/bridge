namespace Bridge;

internal interface IBrokerMessageBus : IMessageBus
{
}

public interface IMessageBus
{
    ValueTask Send<TMessage>(
        TMessage message,
        string queue,
        CancellationToken cancellationToken);

    ValueTask Schedule<TMessage>(
        TMessage message,
        string queue,
        DateTimeOffset enqueueTime,
        CancellationToken cancellationToken);
}