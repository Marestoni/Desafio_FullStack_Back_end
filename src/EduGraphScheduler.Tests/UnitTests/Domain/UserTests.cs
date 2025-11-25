using EduGraphScheduler.Tests.Helpers;
using FluentAssertions;

namespace EduGraphScheduler.Tests.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void User_Should_Have_Empty_CalendarEvents_On_Creation()
    {
        var user = TestDataGenerator.CreateTestUser();

        user.CalendarEvents.Should().BeEmpty();
    }

    [Fact]
    public void User_Should_Calculate_FullName_Correctly()
    {
        var user = TestDataGenerator.CreateTestUser();

        var expectedFullName = $"{user.GivenName} {user.Surname}";

        expectedFullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_Should_Have_Valid_Email()
    {
        var user = TestDataGenerator.CreateTestUser();

        user.Mail.Should().NotBeNullOrEmpty();
        user.Mail.Should().Contain("@");
    }
}