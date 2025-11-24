namespace EduGraphScheduler.Application.Interfaces;

public interface IMicrosoftGraphService
{
    Task<IEnumerable<MicrosoftGraphUser>> GetUsersAsync();
    Task<IEnumerable<MicrosoftGraphEvent>> GetUserEventsAsync(string userPrincipalName);
    Task<string> GetAccessTokenAsync();
    Task<bool> UserHasEventsAsync(string userPrincipalName);
}

public class MicrosoftGraphUser
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string OfficeLocation { get; set; } = string.Empty;
}

public class MicrosoftGraphEvent
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public string OrganizerEmail { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime LastModifiedDateTime { get; set; }
}