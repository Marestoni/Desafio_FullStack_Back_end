namespace EduGraphScheduler.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string OfficeLocation { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; }
    public int EventCount { get; set; }
}