using Azure.Messaging;

namespace Bridge;

internal interface ISerializer
{
    TMessage Convert<TMessage>(CloudEvent cloudEvent);
}