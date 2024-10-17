namespace Bridge;

internal interface IOutboxStorage : IMessageBus
{
    /// <summary>
    /// Add a new outbox item.
    /// </summary>
    ValueTask Add(
        OutboxItem outboxItem,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete and return the next outbox item.
    /// </summary>
    ValueTask<OutboxItem?> Delete(
        CancellationToken cancellationToken);

    /// <summary>
    /// Archives the outbox item.
    /// </summary>
    ValueTask Archive(
        OutboxItem outboxItem,
        CancellationToken cancellationToken);
}