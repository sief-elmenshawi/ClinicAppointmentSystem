using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.Persistence.Seed;

public static class AdminSeeder
{
    private const string AdminEmail = "admin@clinic.com";
    private const string AdminPassword = "P@ssw0rd";

    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingAdmin = await userManager.FindByEmailAsync(AdminEmail);
        if (existingAdmin is not null)
            return; // اتعمل قبل كده، متعملش تاني

        var admin = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}