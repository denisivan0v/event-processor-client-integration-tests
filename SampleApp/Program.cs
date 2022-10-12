using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using SampleApp;

var builder = WebApplication.CreateBuilder(args);

var options = builder.Configuration.GetSection("EventHubs").Get<EventHubsOptions>();
builder.Services
    .AddSingleton(new BlobContainerClient(options.Checkpoint.ConnectionString, options.Checkpoint.ContainerName))
    .AddSingleton(sp =>
    {
        var containerClient = sp.GetRequiredService<BlobContainerClient>();
        return new EventProcessorClient(containerClient, options.ConsumerGroup, options.ConnectionString);
    })
    .AddSingleton<IDataStorage, DataStorage>()
    .AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

public partial class Program { }