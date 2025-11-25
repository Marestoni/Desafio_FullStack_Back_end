using EduGraphScheduler.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduGraphScheduler.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.MicrosoftGraphId)
                  .IsUnique();

            entity.HasIndex(u => u.UserPrincipalName)
                  .IsUnique();

            entity.Property(u => u.DisplayName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(u => u.MicrosoftGraphId)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(u => u.UserPrincipalName)
                  .IsRequired()
                  .HasMaxLength(200);

           entity.Property(u => u.GivenName)
          .IsRequired()
          .HasMaxLength(100);

            entity.Property(u => u.Surname)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(u => u.Mail)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(u => u.MicrosoftGraphId)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(u => u.UserPrincipalName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.HasMany(u => u.CalendarEvents)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.MicrosoftGraphEventId)
                  .IsUnique();

            entity.Property(e => e.Subject)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.MicrosoftGraphEventId)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.OrganizerEmail)
                  .HasMaxLength(255);

            entity.Property(e => e.OrganizerName)
                  .HasMaxLength(255);

            entity.Property(e => e.Location)
                  .HasMaxLength(255);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Start);
            entity.HasIndex(e => e.End);
        });
    }
}