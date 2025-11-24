using EduGraphScheduler.Domain.Entities;

namespace EduGraphScheduler.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMicrosoftGraphIdAsync(string microsoftGraphId);
    Task<IEnumerable<User>> GetUsersWithEventsAsync();
    Task<User?> GetUserWithEventsAsync(Guid userId);
    Task<bool> UserExistsAsync(string microsoftGraphId);
    Task BulkUpsertAsync(IEnumerable<User> users);
    Task<IEnumerable<User>> GetUsersWhoHaveEventsAsync();
}