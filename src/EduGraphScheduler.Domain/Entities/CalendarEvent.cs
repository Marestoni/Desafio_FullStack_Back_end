using System.ComponentModel.DataAnnotations;

namespace EduGraphScheduler.Domain.Entities;

public class CalendarEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string MicrosoftGraphEventId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BodyPreview { get; set; } = string.Empty;

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    public bool IsAllDay { get; set; }

    [MaxLength(100)]
    public string OrganizerEmail { get; set; } = string.Empty;

    [MaxLength(100)]
    public string OrganizerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}