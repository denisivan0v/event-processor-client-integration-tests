namespace SampleApp;

public class EventHubsOptions
{
    public string ConsumerGroup { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public CheckpointOptions Checkpoint { get; set; } = new();
    
    public class CheckpointOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}