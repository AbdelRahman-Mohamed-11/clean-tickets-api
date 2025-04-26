using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Core.Entities;

public class IncidentComment(Guid incidentId, string text, Guid creatorId)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid IncidentId { get; private set; } = incidentId;
    public string Text { get; private set; } = text;
    public Guid CreatorId { get; private set; } = creatorId;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public Incident Incident { get; private set; } = default!;
    public ApplicationUser Creator { get; private set; } = default!;
}
