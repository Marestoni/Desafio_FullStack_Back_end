using EduGraphScheduler.Application.DTOs;

namespace EduGraphScheduler.Application.Interfaces;

public interface ICalendarEventService
{
    Task<IEnumerable<CalendarEventDto>> GetEventsByUserIdAsync(Guid userId);
    Task SyncUserEventsAsync(Guid userId);
    Task SyncAllUsersEventsAsync();
}