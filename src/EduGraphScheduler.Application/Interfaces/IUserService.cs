using EduGraphScheduler.Application.DTOs;

namespace EduGraphScheduler.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserWithEventsDto?> GetUserWithEventsAsync(Guid userId);
    Task SyncUsersFromMicrosoftGraphAsync();
}