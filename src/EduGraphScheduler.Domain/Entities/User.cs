using System.ComponentModel.DataAnnotations;

namespace EduGraphScheduler.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string MicrosoftGraphId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string GivenName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Mail { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UserPrincipalName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(100)]
    public string OfficeLocation { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public int EventCount { get; set; }
    public DateTime? LastEventCheckAt { get; set; }

    // Navigation property
    public virtual ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
}