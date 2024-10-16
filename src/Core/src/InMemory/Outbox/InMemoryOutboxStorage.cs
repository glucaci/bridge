using System.Collections.Concurrent;

namespace Bridge.InMemory;

internal class InMemoryOutboxStorage : IOutboxStorage
{
    internal readonly ConcurrentQueue<OutboxItem> PendingItems = new();
    internal readonly ConcurrentDictionary<string, OutboxItem> ArchivedItems = new();
    
    public ValueTask Add(OutboxItem outboxItem, CancellationToken cancellationToken)
    {
        PendingItems.Enqueue(outboxItem);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask<OutboxItem?> Delete(CancellationToken cancellationToken)
    {
        if (PendingItems.TryDequeue(out var outboxItem))
        {
            return ValueTask.FromResult<OutboxItem?>(outboxItem);
        }

        return ValueTask.FromResult<OutboxItem?>(null);
    }

    public ValueTask Archive(OutboxItem outboxItem, CancellationToken cancellationToken)
    {
        ArchivedItems[outboxItem.Id] = outboxItem;
        return ValueTask.CompletedTask;
    }
}