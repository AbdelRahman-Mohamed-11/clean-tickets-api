using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.Property(i => i.Subject)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(i => i.Description)
               .IsRequired();

        builder.Property(i => i.Suggestion)
               .HasMaxLength(500);

        builder.Property(i => i.UrlOrFormName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(i => i.IsRecurring)
               .IsRequired();

        builder.Property(i => i.RecurringCallId)
               .IsRequired(false);

        builder.Property(i => i.CreatedDate)
               .IsRequired();

        builder.Property(i => i.DeliveryDate)
               .IsRequired(false);

        builder.Property(i => i.CallType)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(i => i.Module)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(i => i.Priority)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(i => i.SupportStatus)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(i => i.UserStatus)
               .HasConversion<string>()
               .IsRequired();

        builder
            .HasOne(i => i.LoggedBy)
            .WithMany(u => u.LoggedIncidents)
            .HasForeignKey(i => i.LoggedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(i => i.AssignedTo)
            .WithMany(u => u.AssignedIncidents)
            .HasForeignKey(i => i.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(i => i.Comments)
            .WithOne(c => c.Incident)
            .HasForeignKey(c => c.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.Attachments)
            .WithOne(a => a.Incident)
            .HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
           .HasQueryFilter(i => !i.IsDeleted);
    }
}
