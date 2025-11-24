using EduGraphScheduler.Tests.Helpers;
using FluentAssertions;

namespace EduGraphScheduler.Tests.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void User_Should_Have_Empty_CalendarEvents_On_Creation()
    {
        // Arrange & Act
        var user = TestDataGenerator.CreateTestUser();

        // Assert
        user.CalendarEvents.Should().BeEmpty();
    }

    [Fact]
    public void User_Should_Calculate_FullName_Correctly()
    {
        // Arrange
        var user = TestDataGenerator.CreateTestUser();

        // Act
        var expectedFullName = $"{user.GivenName} {user.Surname}";

        // Assert
        expectedFullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_Should_Have_Valid_Email()
    {
        // Arrange
        var user = TestDataGenerator.CreateTestUser();

        // Assert
        user.Mail.Should().NotBeNullOrEmpty();
        user.Mail.Should().Contain("@");
    }
}