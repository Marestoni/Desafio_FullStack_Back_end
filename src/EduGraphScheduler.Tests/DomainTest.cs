using Xunit;
using FluentAssertions;
using EduGraphUser = EduGraphScheduler.Domain.Entities.User;
using EduGraphCalendarEvent = EduGraphScheduler.Domain.Entities.CalendarEvent;

namespace EduGraphScheduler.Tests;

public class DomainTest
{
    [Fact]
    public void Can_Create_User_Entity()
    {
        // Arrange & Act
        var user = new EduGraphUser
        {
            Id = Guid.NewGuid(),
            MicrosoftGraphId = "test-123",
            DisplayName = "Test User",
            UserPrincipalName = "test@edu.com",
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.DisplayName.Should().Be("Test User");
        user.CalendarEvents.Should().BeEmpty();
    }

    [Fact]
    public void Can_Create_CalendarEvent_Entity()
    {
        // Arrange & Act
        var calendarEvent = new EduGraphCalendarEvent
        {
            Id = Guid.NewGuid(),
            MicrosoftGraphEventId = "event-123",
            Subject = "Test Event",
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow.AddHours(1),
            UserId = Guid.NewGuid()
        };

        // Assert
        calendarEvent.Should().NotBeNull();
        calendarEvent.Subject.Should().Be("Test Event");
        calendarEvent.Start.Should().BeBefore(calendarEvent.End);
    }
}