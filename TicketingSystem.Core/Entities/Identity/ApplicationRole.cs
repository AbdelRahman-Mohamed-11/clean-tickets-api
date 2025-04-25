using Microsoft.AspNetCore.Identity;

namespace TicketingSystem.Core.Entities.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Description { get; set; } = default!;

        public bool IsDeleted { get; set; } = false;

    }
}
