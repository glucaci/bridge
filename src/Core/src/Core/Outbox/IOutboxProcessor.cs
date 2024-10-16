namespace Bridge;

internal interface IOutboxProcessor
{
    Task Execute(CancellationToken cancellationToken);
}