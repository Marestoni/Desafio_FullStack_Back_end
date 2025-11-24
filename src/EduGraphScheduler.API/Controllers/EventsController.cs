using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduGraphScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ICalendarEventService _eventService;

    public EventsController(ICalendarEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetUserEvents(Guid userId)
    {
        var events = await _eventService.GetEventsByUserIdAsync(userId);
        return Ok(events);
    }

    [HttpPost("sync/user/{userId}")]
    public async Task<ActionResult> SyncUserEvents(Guid userId)
    {
        await _eventService.SyncUserEventsAsync(userId);
        return Ok(new { message = $"Events synchronization started for user {userId}" });
    }

    [HttpPost("sync/all")]
    public async Task<ActionResult> SyncAllEvents()
    {
        await _eventService.SyncAllUsersEventsAsync();
        return Ok(new { message = "Events synchronization started for all users" });
    }
}