using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Persistence.Configurations;

namespace TicketingSystem.Infrastructure.Persistence
{
    public class TicketDbContext(DbContextOptions options)
                : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), ITicketDbContext
    {
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentComment> IncidentComments { get; set; }
        public DbSet<IncidentAttachment> IncidentAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationRoleConfiguration).Assembly);

            builder.Ignore<IdentityUserLogin<Guid>>();
            builder.Ignore<IdentityUserToken<Guid>>();


        }
    }
}
