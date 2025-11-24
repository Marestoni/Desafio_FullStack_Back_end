namespace EduGraphScheduler.Application.DTOs;

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public string OrganizerEmail { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
}