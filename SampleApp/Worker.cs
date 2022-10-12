using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;

namespace SampleApp;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDataStorage _dataStorage;
    private readonly EventProcessorClient _eventProcessorClient;

    public Worker(ILogger<Worker> logger, EventProcessorClient eventProcessorClient, IDataStorage dataStorage)
    {
        _logger = logger;
        _dataStorage = dataStorage;
        _eventProcessorClient = eventProcessorClient;
        _eventProcessorClient.PartitionInitializingAsync += OnPartitionInitializingAsync;
        _eventProcessorClient.PartitionClosingAsync += OnPartitionClosingAsync;
        _eventProcessorClient.ProcessEventAsync += OnProcessEventAsync;
        _eventProcessorClient.ProcessErrorAsync += ClientOnProcessErrorAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventProcessorClient.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _eventProcessorClient.StopProcessingAsync(cancellationToken);
    }

    private Task OnPartitionInitializingAsync(PartitionInitializingEventArgs arg)
    {
        _logger.LogInformation("Initializing partition {partitionId}...", arg.PartitionId);
        return Task.CompletedTask;
    }
    
    private Task OnPartitionClosingAsync(PartitionClosingEventArgs arg)
    {
        _logger.LogInformation("Closing partition {partitionId} due to {reason}... ", arg.PartitionId, arg.Reason.ToString());
        return Task.CompletedTask;
    }
    
    private async Task OnProcessEventAsync(ProcessEventArgs arg)
    {
        if (arg.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Processing event # {sequenceNumber} from partition {partitionId}...",
                arg.Data.SequenceNumber, arg.Partition.PartitionId);
            _dataStorage.Buffer(arg.Data.Body);

            await arg.UpdateCheckpointAsync(arg.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing event # {sequenceNumber} from partition {partitionId}...", arg.Data.SequenceNumber, arg.Partition.PartitionId);
            throw;
        }
    }
    
    private Task ClientOnProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Error while receiving events.");
        return Task.CompletedTask;
    }
}