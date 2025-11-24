using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;

namespace EduGraphScheduler.Application.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMicrosoftGraphService _microsoftGraphService;

    public CalendarEventService(
        ICalendarEventRepository eventRepository,
        IUserRepository userRepository,
        IMicrosoftGraphService microsoftGraphService)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _microsoftGraphService = microsoftGraphService;
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

        var graphEvents = await _microsoftGraphService.GetUserEventsAsync(user.UserPrincipalName);

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
    }

    public async Task SyncAllUsersEventsAsync()
    {
        var users = await _userRepository.GetAllAsync();

        foreach (var user in users)
        {
            try
            {
                await SyncUserEventsAsync(user.Id);
            }
            catch (Exception ex)
            {
                // Log the error but continue with other users
                Console.WriteLine($"Error syncing events for user {user.UserPrincipalName}: {ex.Message}");
            }
        }
    }
}