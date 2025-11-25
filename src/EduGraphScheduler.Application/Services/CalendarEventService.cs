using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduGraphScheduler.Application.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMicrosoftGraphService _microsoftGraphService;
    private readonly ILogger<CalendarEventService> _logger;


    public CalendarEventService(
        ICalendarEventRepository eventRepository,
        IUserRepository userRepository,
        IMicrosoftGraphService microsoftGraphService,
        ILogger<CalendarEventService> logger)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _microsoftGraphService = microsoftGraphService;
        _logger = logger;
    }

    public async Task<IEnumerable<CalendarEventDto>> GetEventsByUserIdAsync(Guid userId)
    {
        var events = await _eventRepository.GetEventsByUserIdAsync(userId);

        return events.Select(e => new CalendarEventDto
        {
            Id = e.Id,
            Subject = e.Subject,
            BodyPreview = e.BodyPreview,
            Start = e.Start,
            End = e.End,
            Location = e.Location,
            IsAllDay = e.IsAllDay,
            OrganizerEmail = e.OrganizerEmail,
            OrganizerName = e.OrganizerName,
            LastUpdatedAt = e.LastUpdatedAt
        });
    }

    public async Task SyncUserEventsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        _logger.LogInformation("Syncing events for user: {UserPrincipalName}", user.UserPrincipalName);

        try
        {
            var graphEvents = await _microsoftGraphService.GetUserEventsAsync(user.UserPrincipalName);

            _logger.LogInformation("Retrieved {Count} events for user {UserPrincipalName}",
                graphEvents.Count(), user.UserPrincipalName);

            var events = graphEvents.Select(ge => new CalendarEvent
            {
                MicrosoftGraphEventId = ge.Id,
                Subject = ge.Subject,
                BodyPreview = ge.BodyPreview,
                Start = ge.Start,
                End = ge.End,
                Location = ge.Location,
                IsAllDay = ge.IsAllDay,
                OrganizerEmail = ge.OrganizerEmail,
                OrganizerName = ge.OrganizerName,
                UserId = userId,
                LastUpdatedAt = DateTime.UtcNow
            });

            await _eventRepository.BulkUpsertAsync(events);

            _logger.LogInformation("Events synchronization completed for user: {UserPrincipalName}. Synced {Count} events.",
                user.UserPrincipalName, events.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing events for user: {UserPrincipalName}", user.UserPrincipalName);
            throw;
        }
    }

    public async Task SyncAllUsersEventsAsync()
    {
        _logger.LogInformation("Starting CACHED events sync");

        var allUsers = await _userRepository.GetAllAsync();
        var totalUsers = allUsers.Count();

        var usersToCheck = allUsers
            .Where(u => !string.IsNullOrEmpty(u.UserPrincipalName))
            .Where(u => !u.LastEventCheckAt.HasValue ||
                       u.LastEventCheckAt.Value < DateTime.UtcNow.AddDays(-1))
            .ToList();

        _logger.LogInformation("Will check {ToCheck}/{Total} users for events", usersToCheck.Count, totalUsers);

        var usersWithEvents = 0;
        var processed = 0;

        foreach (var user in usersToCheck)
        {
            try
            {
                user.LastEventCheckAt = DateTime.UtcNow;

                var hasEvents = await _microsoftGraphService.UserHasEventsAsync(user.UserPrincipalName);

                if (hasEvents)
                {
                    usersWithEvents++;

                    await SyncUserEventsAsync(user.Id);

                    _logger.LogDebug("Synced events for user {UserPrincipalName}", user.UserPrincipalName);
                }
                else
                {
                    // Marca como sincronizado (sem eventos)
                    user.LastSyncedAt = DateTime.UtcNow;
                    user.EventCount = 0;
                    _logger.LogDebug("No events for user {UserPrincipalName}", user.UserPrincipalName);
                }

                await _userRepository.UpdateAsync(user);
                processed++;

                // Log a cada 100 usuários verificados (mais frequente para debug)
                if (processed % 100 == 0)
                {
                    _logger.LogInformation("Check progress: {Processed}/{Total} - {WithEvents} have events",
                        processed, usersToCheck.Count, usersWithEvents);
                }

                // Pequena pausa para evitar rate limits
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user {UserPrincipalName}", user.UserPrincipalName);
            }
        }

        _logger.LogInformation("CACHED sync completed: {WithEvents} users have events out of {Checked} checked",
            usersWithEvents, processed);
    }


}