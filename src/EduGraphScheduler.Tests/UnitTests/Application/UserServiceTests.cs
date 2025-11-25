using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Application.Services;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using EduGraphScheduler.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace EduGraphScheduler.Tests.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMicrosoftGraphService> _microsoftGraphServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _microsoftGraphServiceMock = new Mock<IMicrosoftGraphService>();
        _userService = new UserService(_userRepositoryMock.Object, _microsoftGraphServiceMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_Should_Return_UserDtos()
    {
        var testUser = TestDataGenerator.CreateTestUser();
        var testEvent = TestDataGenerator.CreateTestCalendarEvent(testUser.Id);
        testUser.CalendarEvents.Add(testEvent);

        var users = new List<User> { testUser };

        _userRepositoryMock
            .Setup(repo => repo.GetUsersWithEventsAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var userDto = result.First();
        userDto.Id.Should().Be(testUser.Id);
        userDto.DisplayName.Should().Be(testUser.DisplayName);
        userDto.EventCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUserWithEventsAsync_Should_Return_Null_For_NonExistent_User()
    {
        var nonExistentUserId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(repo => repo.GetUserWithEventsAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        var result = await _userService.GetUserWithEventsAsync(nonExistentUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SyncUsersFromMicrosoftGraphAsync_Should_Call_BulkUpsert()
    {
        var graphUsers = new List<MicrosoftGraphUser>
        {
            new() {
                Id = "graph-user-1",
                DisplayName = "Graph User 1",
                GivenName = "Graph",
                Surname = "User",
                Mail = "graph.user1@edu.com",
                UserPrincipalName = "graph.user1@edu.com",
                JobTitle = "Teacher",
                Department = "Math",
                OfficeLocation = "Building B"
            }
        };

        _microsoftGraphServiceMock
            .Setup(service => service.GetUsersAsync())
            .ReturnsAsync(graphUsers);

        _userRepositoryMock
            .Setup(repo => repo.BulkUpsertAsync(It.IsAny<IEnumerable<User>>()))
            .Returns(Task.CompletedTask);

        await _userService.SyncUsersFromMicrosoftGraphAsync();

        _userRepositoryMock.Verify(
            repo => repo.BulkUpsertAsync(It.IsAny<IEnumerable<User>>()),
            Times.Once);
    }
}