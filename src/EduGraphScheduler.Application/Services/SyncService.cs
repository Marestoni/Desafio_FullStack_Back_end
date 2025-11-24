using EduGraphScheduler.Application.Interfaces;

namespace EduGraphScheduler.Application.Services;

public class SyncService : ISyncService
{
    private readonly IUserService _userService;
    private readonly ICalendarEventService _eventService;

    public SyncService(IUserService userService, ICalendarEventService eventService)
    {
        _userService = userService;
        _eventService = eventService;
    }

    public async Task SyncAllDataAsync()
    {
        await SyncUsersAsync();
        await _eventService.SyncAllUsersEventsAsync();
    }

    public async Task SyncUsersAsync()
    {
        await _userService.SyncUsersFromMicrosoftGraphAsync();
    }

    public async Task SyncUserEventsAsync(string userPrincipalName)
    {
        await Task.CompletedTask; // Placeholder
    }
}