
using DhakaRent.DataAccess.Identity;
using Microsoft.AspNetCore.Identity;

namespace DhakaRent.Web.UI.Identity
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            // Read Admin credentials from User Secrets
            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];

            // Skip seeding if credentials are not configured
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            using var scope = services.CreateScope();

            var serviceProvider = scope.ServiceProvider;

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminRole = "Admin";

            // Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                throw new InvalidOperationException(
                    "Admin role does not exist. Run RoleSeeder first.");
            }

            // Find existing Admin account by email
            var adminUser = await userManager.FindByEmailAsync(email);

            // Create account if it does not exist
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult =
                    await userManager.CreateAsync(adminUser, password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        createResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Admin account creation failed: {errors}");
                }
            }

            // Assign Admin role if not already assigned
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(adminUser, adminRole);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Admin role assignment failed: {errors}");
                }
            }
        }
    }
}
