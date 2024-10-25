using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureCloudEvent = Azure.Messaging.CloudEvent;
using NativeCloudEvent = CloudNative.CloudEvents.CloudEvent;

namespace Bridge.Bus.AzureServiceBus;

internal class AzureServiceBusProcessor : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfiguration _consumerConfiguration;
    private readonly ILogger<AzureServiceBusProcessor> _logger;

    internal AzureServiceBusProcessor(
        IServiceProvider serviceProvider,
        ConsumerConfiguration consumerConfiguration)
    {
        _serviceProvider = serviceProvider;
        _consumerConfiguration = consumerConfiguration;
        _logger = serviceProvider.GetRequiredService<ILogger<AzureServiceBusProcessor>>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ServiceBusProcessor client = CreateServiceBusProcessor();

        client.ProcessMessageAsync += HandleMessage;
        client.ProcessErrorAsync += HandleError;

        await client.StartProcessingAsync(cancellationToken);
    }

    private Task HandleError(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Error processing on entity: {EntityPath}", arg.EntityPath);
        return Task.CompletedTask;
    }

    private async Task HandleMessage(ProcessMessageEventArgs arg)
    {
        AzureCloudEvent? azureCloudEvent = AzureCloudEvent.Parse(arg.Message.Body);
        if (azureCloudEvent == null)
        {
            await arg.DeadLetterMessageAsync(arg.Message);
        }
        else
        {
            var cloudNativeEvent = new NativeCloudEvent(CloudEventsSpecVersion.V1_0)
            {
                Id = azureCloudEvent.Id,
                Source = new Uri(azureCloudEvent.Source, UriKind.RelativeOrAbsolute),
                Type = azureCloudEvent.Type,
                Time = azureCloudEvent.Time,
                DataContentType = azureCloudEvent.DataContentType,
                DataSchema = azureCloudEvent.DataSchema != null ? 
                    new Uri(azureCloudEvent.DataSchema, UriKind.RelativeOrAbsolute) : 
                    null,
                Subject = azureCloudEvent.Subject,
                Data = azureCloudEvent.Data
            };

            foreach (var attribute in azureCloudEvent.ExtensionAttributes)
            {
                cloudNativeEvent[attribute.Key] = attribute.Value;
            }
            
            await _consumerConfiguration.HandleMessage(_serviceProvider, cloudNativeEvent, arg.CancellationToken);
            await arg.CompleteMessageAsync(arg.Message);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ServiceBusProcessor client = CreateServiceBusProcessor();

        client.ProcessMessageAsync -= HandleMessage;
        client.ProcessErrorAsync -= HandleError;

        await client.StopProcessingAsync(cancellationToken);
        await client.DisposeAsync();
    }

    private ServiceBusProcessor CreateServiceBusProcessor()
    {
        IAzureClientFactory<ServiceBusProcessor> serviceBusClientFactory = _serviceProvider
            .GetRequiredService<IAzureClientFactory<ServiceBusProcessor>>();

        return serviceBusClientFactory
            .CreateClient(_consumerConfiguration.QueueName);
    }
}