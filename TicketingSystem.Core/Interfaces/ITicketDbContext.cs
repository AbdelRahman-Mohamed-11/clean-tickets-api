using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces;

public interface ITicketDbContext
{
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentComment> IncidentComments { get; }
    DbSet<IncidentAttachment> IncidentAttachments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
