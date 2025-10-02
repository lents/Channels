using Microsoft.AspNetCore.Mvc;
using RealWorld.WebApi.Services;

namespace RealWorld.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;
    private readonly ChannelService _channelService;

    public JobsController(ILogger<JobsController> logger, ChannelService channelService)
    {
        _logger = logger;
        _channelService = channelService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TaskName))
        {
            return BadRequest("TaskName is required.");
        }

        var jobId = Guid.NewGuid().ToString();
        var job = new Job(jobId, request.TaskName);

        // Write the job to the channel. This is a fast, non-blocking operation.
        await _channelService.Writer.WriteAsync(job);

        _logger.LogInformation("Job {JobId} has been queued.", jobId);

        // Return a 202 Accepted response. This tells the client that the work has been
        // accepted for processing, but the processing has not been completed.
        // The client can use the jobId to check the status of the job later.
        return Accepted(new { JobId = jobId });
    }
}