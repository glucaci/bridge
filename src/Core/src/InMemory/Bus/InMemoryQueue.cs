using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Bridge.InMemory;

internal class InMemoryQueue<T>
{
    private readonly Channel<T> _inner;
    private readonly Queue<T> _items = new();

    public InMemoryQueue(Channel<T> inner)
    {
        _inner = inner;
    }

    public Task Waiter => _inner.Reader.Completion;

    public ValueTask Enqueue(T item, CancellationToken cancellationToken)
    {
        _items.Enqueue(item);
        return _inner.Writer.WriteAsync(item, cancellationToken);
    }

    public IReadOnlyList<T> GetItems() => _items.ToList();

    public ValueTask<bool> WaitForMessages(CancellationToken cancellationToken)
    {
        return _inner.Reader.WaitToReadAsync(cancellationToken);
    }

    public bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        return _inner.Reader.TryPeek(out item);
    }

    public ValueTask<T> Dequeue(CancellationToken cancellationToken)
    {
        return _inner.Reader.ReadAsync(cancellationToken);
    }

    public void Close()
    {
        _inner.Writer.Complete();
    }
}