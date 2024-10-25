namespace Bridge.Bus;

internal class ConsumerConfigurationBuilder
{
    public ConsumerConfigurationBuilder(
        string queueName,
        Func<IServiceProvider, ConsumerConfiguration> create)
    {
        QueueName = queueName;
        Create = create;
    }
    
    internal string QueueName { get; }
    
    internal Func<IServiceProvider, ConsumerConfiguration> Create { get; }
}