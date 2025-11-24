using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using EduGraphScheduler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduGraphScheduler.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByMicrosoftGraphIdAsync(string microsoftGraphId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.MicrosoftGraphId == microsoftGraphId);
    }

    public async Task<IEnumerable<User>> GetUsersWithEventsAsync()
    {
        return await _context.Users
            .Include(u => u.CalendarEvents)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<User?> GetUserWithEventsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.CalendarEvents
                .OrderBy(e => e.Start)
                .ThenBy(e => e.End))
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UserExistsAsync(string microsoftGraphId)
    {
        return await _context.Users
            .AnyAsync(u => u.MicrosoftGraphId == microsoftGraphId);
    }

    public async Task BulkUpsertAsync(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            var existingUser = await GetByMicrosoftGraphIdAsync(user.MicrosoftGraphId);

            if (existingUser != null)
            {
                // Update existing user
                existingUser.DisplayName = user.DisplayName;
                existingUser.GivenName = user.GivenName;
                existingUser.Surname = user.Surname;
                existingUser.Mail = user.Mail;
                existingUser.JobTitle = user.JobTitle;
                existingUser.Department = user.Department;
                existingUser.OfficeLocation = user.OfficeLocation;
                existingUser.LastSyncedAt = DateTime.UtcNow;

                _context.Users.Update(existingUser);
            }
            else
            {
                // Add new user
                await _context.Users.AddAsync(user);
            }
        }

        await _context.SaveChangesAsync();
    }
}