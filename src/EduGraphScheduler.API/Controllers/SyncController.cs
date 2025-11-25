using EduGraphScheduler.Application.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduGraphScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        IRecurringJobManager recurringJobManager,
        ISyncService syncService,
        ILogger<SyncController> logger)
    {
        _recurringJobManager = recurringJobManager;
        _syncService = syncService;
        _logger = logger;
    }

    [HttpPost("start")]
    public IActionResult StartSync()
    {
        _logger.LogInformation("Manual sync triggered");
        BackgroundJob.Enqueue<ISyncService>(x => x.SyncAllDataAsync());

        return Ok(new { message = "Sync job enqueued successfully" });
    }

    [HttpPost("schedule")]
    public IActionResult ScheduleSync([FromQuery] string cronExpression = "0 */6 * * *")
    {
        _logger.LogInformation("Scheduling recurring sync with cron: {CronExpression}", cronExpression);

        _recurringJobManager.AddOrUpdate<ISyncService>(
            "full-data-sync",
            x => x.SyncAllDataAsync(),
            cronExpression);

        return Ok(new
        {
            message = "Recurring sync scheduled successfully",
            cronExpression = cronExpression
        });
    }

    [HttpPost("users")]
    public IActionResult SyncUsers()
    {
        _logger.LogInformation("Manual users sync triggered");

        BackgroundJob.Enqueue<ISyncService>(x => x.SyncUsersAsync());

        return Ok(new { message = "Users sync job enqueued successfully" });
    }
}