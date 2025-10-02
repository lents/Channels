namespace RealWorld.WebApi.Services;

public class BackgroundJobService : BackgroundService
{
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly ChannelService _channelService;

    public BackgroundJobService(ILogger<BackgroundJobService> logger, ChannelService channelService)
    {
        _logger = logger;
        _channelService = channelService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Job Service is starting.");

        // Asynchronously read from the channel until it's completed and the token is cancelled.
        await foreach (var job in _channelService.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Processing Job: {JobId}, Task: {TaskName}", job.Id, job.TaskName);

            try
            {
                // Simulate some long-running, asynchronous work.
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                _logger.LogInformation("Finished processing Job: {JobId}", job.Id);
            }
            catch (OperationCanceledException)
            {
                // The stopping token was triggered.
                _logger.LogWarning("Job processing was cancelled for JobId: {JobId}.", job.Id);
                break; // Exit the loop.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing JobId: {JobId}.", job.Id);
            }
        }

        _logger.LogInformation("Background Job Service is stopping.");
    }
}