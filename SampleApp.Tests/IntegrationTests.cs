using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SampleApp.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IDataStorage> _storageMock = new();
    private readonly Mock<EventProcessorClient> _processorMock = new();
    
    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _storageMock.Setup(x => x.Buffer(It.IsAny<ReadOnlyMemory<byte>>())).Verifiable();
        
        _ = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_processorMock.Object);
                services.AddSingleton(_storageMock.Object);
            });
        }).CreateDefaultClient();
    }
    
    [Fact]
    public void Run()
    {
        // Here we are mocking a partition context using the model factory.
        var partitionContext = EventHubsModelFactory.PartitionContext(
            fullyQualifiedNamespace: "sample-hub.servicebus.windows.net",
            eventHubName: "sample-hub",
            consumerGroup: "$Default",
            partitionId: "0");

        // Here we are mocking an event data instance with broker-owned properties populated.
        var eventData = EventHubsModelFactory.EventData(
            eventBody: new BinaryData("Sample-Event"),
            systemProperties: new Dictionary<string, object>(), //arbitrary value
            partitionKey: "sample-key",
            sequenceNumber: 1000,
            offset: 1500,
            enqueuedTime: DateTimeOffset.Parse("11:36 PM"));

        // This creates a new instance of ProcessEventArgs to pass into the handler directly.
        ProcessEventArgs processEventArgs = new(
            partition: partitionContext,
            data: eventData,
            updateCheckpointImplementation: async _ => await Task.CompletedTask); // arbitrary value
        
        _processorMock.Raise(x => x.ProcessEventAsync += null, processEventArgs); // <-- This cannot be done since ProcessEventAsync is not virtual
        
        _storageMock.Verify(x => x.Buffer(It.IsAny<ReadOnlyMemory<byte>>()), Times.AtLeastOnce);
    }
}