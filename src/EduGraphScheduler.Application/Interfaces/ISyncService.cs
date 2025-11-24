namespace EduGraphScheduler.Application.Interfaces;

public interface ISyncService
{
    Task SyncAllDataAsync();
    Task SyncUsersAsync();
    Task SyncUserEventsAsync(string userPrincipalName);
}