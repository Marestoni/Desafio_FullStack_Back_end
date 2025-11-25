using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduGraphScheduler.Application.Services;

public class SyncService : ISyncService
{
    private readonly IUserService _userService;
    private readonly ICalendarEventService _eventService;
    private readonly IMicrosoftGraphService _microsoftGraphService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
    IUserService userService,
    ICalendarEventService eventService,
    IUserRepository userRepository,
    IMicrosoftGraphService microsoftGraphService,
    ILogger<SyncService> logger)
    {
        _userService = userService;
        _eventService = eventService;
        _userRepository = userRepository;
        _microsoftGraphService = microsoftGraphService;
        _logger = logger;
    }

    public async Task SyncAllDataAsync()
    {
        _logger.LogInformation("Starting full data synchronization from Microsoft Graph");

        try
        {
            await SyncUsersAsync();

            var users = await _userRepository.GetAllAsync();

            _logger.LogInformation("Starting events synchronization for {Count} users", users.Count());

            foreach (var user in users)
            {
                try
                {
                    await _eventService.SyncUserEventsAsync(user.Id);
                    _logger.LogDebug("Events synchronized for user: {UserPrincipalName}", user.UserPrincipalName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync events for user: {UserPrincipalName}", user.UserPrincipalName);
                }
            }

            _logger.LogInformation("Full data synchronization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full data synchronization from Microsoft Graph");
            throw;
        }
    }

    public async Task SyncUsersAsync()
    {
        _logger.LogInformation("Starting users synchronization from Microsoft Graph");

        try
        {
            var graphUsers = await _microsoftGraphService.GetUsersAsync();

            _logger.LogInformation("Retrieved {Count} users from Microsoft Graph", graphUsers.Count());

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

            _logger.LogInformation("Users synchronization completed successfully. Synced {Count} users.", users.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during users synchronization from Microsoft Graph");
            throw;
        }
    }

    public async Task SyncUserEventsAsync(string userPrincipalName)
    {
        await Task.CompletedTask;
    }
}