using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using EduGraphScheduler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduGraphScheduler.Infrastructure.Repositories;

public class CalendarEventRepository : BaseRepository<CalendarEvent>, ICalendarEventRepository
{
    public CalendarEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsByUserIdAsync(Guid userId)
    {
        return await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Start)
            .ThenBy(e => e.End)
            .ToListAsync();
    }

    public async Task<CalendarEvent?> GetByMicrosoftGraphEventIdAsync(string eventId)
    {
        return await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.MicrosoftGraphEventId == eventId);
    }

    public async Task<bool> EventExistsAsync(string microsoftGraphEventId)
    {
        return await _context.CalendarEvents
            .AnyAsync(e => e.MicrosoftGraphEventId == microsoftGraphEventId);
    }

    public async Task BulkUpsertAsync(IEnumerable<CalendarEvent> events)
    {
        foreach (var calendarEvent in events)
        {
            var existingEvent = await GetByMicrosoftGraphEventIdAsync(calendarEvent.MicrosoftGraphEventId);

            if (existingEvent != null)
            {
                existingEvent.Subject = calendarEvent.Subject;
                existingEvent.BodyPreview = calendarEvent.BodyPreview;
                existingEvent.Start = calendarEvent.Start;
                existingEvent.End = calendarEvent.End;
                existingEvent.Location = calendarEvent.Location;
                existingEvent.IsAllDay = calendarEvent.IsAllDay;
                existingEvent.OrganizerEmail = calendarEvent.OrganizerEmail;
                existingEvent.OrganizerName = calendarEvent.OrganizerName;
                existingEvent.LastUpdatedAt = DateTime.UtcNow;

                _context.CalendarEvents.Update(existingEvent);
            }
            else
            {
                await _context.CalendarEvents.AddAsync(calendarEvent);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteEventsByUserIdAsync(Guid userId)
    {
        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .ToListAsync();

        _context.CalendarEvents.RemoveRange(events);
        await _context.SaveChangesAsync();
    }
}