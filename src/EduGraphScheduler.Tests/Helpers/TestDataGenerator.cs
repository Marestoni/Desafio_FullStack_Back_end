using EduGraphScheduler.Domain.Entities;

namespace EduGraphScheduler.Tests.Helpers;

public static class TestDataGenerator
{
    public static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            MicrosoftGraphId = "test-user-123",
            DisplayName = "John Doe",
            GivenName = "John",
            Surname = "Doe",
            Mail = "john.doe@edu.com",
            UserPrincipalName = "john.doe@edu.com",
            JobTitle = "Professor",
            Department = "Computer Science",
            OfficeLocation = "Building A",
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
    }

    public static CalendarEvent CreateTestCalendarEvent(Guid userId)
    {
        return new CalendarEvent
        {
            Id = Guid.NewGuid(),
            MicrosoftGraphEventId = "test-event-123",
            Subject = "Test Meeting",
            BodyPreview = "This is a test meeting",
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            Location = "Room 101",
            IsAllDay = false,
            OrganizerEmail = "organizer@edu.com",
            OrganizerName = "Meeting Organizer",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
    }
}