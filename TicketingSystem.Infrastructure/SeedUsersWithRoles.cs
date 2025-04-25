using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Infrastructure
{
    public static class SeedUsersWithRoles
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            foreach (Role roleEnum in Enum.GetValues<Role>())
            {
                var roleName = roleEnum.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var description = roleEnum switch
                    {
                        Role.Admin => "Administrator with full access to all incidents",
                        Role.ERP => "ERP team member with full visibility and commenting rights",
                        Role.User => "Normal user, can manage only own incidents",
                        _ => string.Empty
                    };

                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = description
                    });
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            await SeedUserAsync(userManager, "admin", "admin@ticketing.local", "Admin123!@", nameof(Role.Admin));
            await SeedUserAsync(userManager, "erpuser", "erp@ticketing.local", "ERPp123!@", nameof(Role.ERP));
            await SeedUserAsync(userManager, "normaluser", "user@ticketing.local", "User123!@", nameof(Role.User));
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string userName,
            string email,
            string password,
            string role)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
