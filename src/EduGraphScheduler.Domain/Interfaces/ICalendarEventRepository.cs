using EduGraphScheduler.Domain.Entities;

namespace EduGraphScheduler.Domain.Interfaces;

public interface ICalendarEventRepository : IRepository<CalendarEvent>
{
    Task<IEnumerable<CalendarEvent>> GetEventsByUserIdAsync(Guid userId);
    Task<CalendarEvent?> GetByMicrosoftGraphEventIdAsync(string eventId);
    Task<bool> EventExistsAsync(string microsoftGraphEventId);
    Task BulkUpsertAsync(IEnumerable<CalendarEvent> events);
    Task DeleteEventsByUserIdAsync(Guid userId);
}