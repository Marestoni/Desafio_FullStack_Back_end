namespace EduGraphScheduler.Application.DTOs;

public class UserWithEventsDto
{
    public UserDto User { get; set; } = new();
    public List<CalendarEventDto> Events { get; set; } = new();
}