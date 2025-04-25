using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Core.Entities;

public class IncidentComment
{
    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public string Text { get; private set; } = default!;
    public Guid CreatorId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public Incident Incident { get; private set; } = default!;
    public ApplicationUser Creator { get; private set; } = default!;
}
