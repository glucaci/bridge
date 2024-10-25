using CloudNative.CloudEvents;

namespace Bridge.Bus;

internal interface ISerializer
{
    TMessage Convert<TMessage>(CloudEvent cloudEvent);
}