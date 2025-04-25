using Microsoft.AspNetCore.Identity;

namespace TicketingSystem.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? RefreshTokenExpiryDate { get; set; }
}
