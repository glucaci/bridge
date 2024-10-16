using Azure;
using Azure.Messaging;

namespace Bridge;

internal class DefaultSerializer : ISerializer
{
    public TMessage Convert<TMessage>(CloudEvent cloudEvent)
    {
        return cloudEvent.Data.ToObjectFromJson<TMessage>();
    }
}