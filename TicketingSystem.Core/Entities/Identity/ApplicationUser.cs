using Microsoft.AspNetCore.Identity;

namespace TicketingSystem.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? RefreshTokenExpiryDate { get; set; }

    public virtual ICollection<Incident> LoggedIncidents { get; set; } = [];

    public virtual ICollection<Incident> AssignedIncidents { get; set; } = [];

    public virtual ICollection<IncidentComment> Comments { get; set; } = [];

    public virtual ICollection<IncidentAttachment> Attachments { get; set; } = [];
}
