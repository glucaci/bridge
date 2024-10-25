using CloudNative.CloudEvents;

namespace Bridge.Bus;

internal class DefaultSerializer : ISerializer
{
    public TMessage Convert<TMessage>(CloudEvent cloudEvent)
    {
        return (TMessage)cloudEvent.Data!;
    }
}