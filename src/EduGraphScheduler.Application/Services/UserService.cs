using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;

namespace EduGraphScheduler.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMicrosoftGraphService _microsoftGraphService;

    public UserService(IUserRepository userRepository, IMicrosoftGraphService microsoftGraphService)
    {
        _userRepository = userRepository;
        _microsoftGraphService = microsoftGraphService;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetUsersWithEventsAsync();

        return users.Select(user => new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            GivenName = user.GivenName,
            Surname = user.Surname,
            Mail = user.Mail,
            JobTitle = user.JobTitle,
            Department = user.Department,
            OfficeLocation = user.OfficeLocation,
            LastSyncedAt = user.LastSyncedAt,
            EventCount = user.CalendarEvents.Count
        });
    }

    public async Task<UserWithEventsDto?> GetUserWithEventsAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithEventsAsync(userId);

        if (user == null)
            return null;

        var userDto = new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            GivenName = user.GivenName,
            Surname = user.Surname,
            Mail = user.Mail,
            JobTitle = user.JobTitle,
            Department = user.Department,
            OfficeLocation = user.OfficeLocation,
            LastSyncedAt = user.LastSyncedAt,
            EventCount = user.CalendarEvents.Count
        };

        var eventDtos = user.CalendarEvents.Select(e => new CalendarEventDto
        {
            Id = e.Id,
            Subject = e.Subject,
            BodyPreview = e.BodyPreview,
            Start = e.Start,
            End = e.End,
            Location = e.Location,
            IsAllDay = e.IsAllDay,
            OrganizerEmail = e.OrganizerEmail,
            OrganizerName = e.OrganizerName,
            LastUpdatedAt = e.LastUpdatedAt
        }).ToList();

        return new UserWithEventsDto
        {
            User = userDto,
            Events = eventDtos
        };
    }

    public async Task SyncUsersFromMicrosoftGraphAsync()
    {
        var graphUsers = await _microsoftGraphService.GetUsersAsync();

        var users = graphUsers.Select(gu => new User
        {
            MicrosoftGraphId = gu.Id,
            DisplayName = gu.DisplayName,
            GivenName = gu.GivenName,
            Surname = gu.Surname,
            Mail = gu.Mail,
            UserPrincipalName = gu.UserPrincipalName,
            JobTitle = gu.JobTitle,
            Department = gu.Department,
            OfficeLocation = gu.OfficeLocation,
            LastSyncedAt = DateTime.UtcNow
        });

        await _userRepository.BulkUpsertAsync(users);
    }
}