using Clinic.Application.Interfaces;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? UserId, string? Error)> CreateUserAsync(
        string email, string password, string role)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, null, errors);
        }

        await _userManager.AddToRoleAsync(user, role);
        return (true, user.Id, null);
    }

    public async Task<(bool Succeeded, string? UserId, IList<string>? Roles, string? Error)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return (false, null, null, "Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
            return (false, null, null, "Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);

        return (true, user.Id, roles, null);
    }
    public async Task<string?> GetUserEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.Email;
    }
    public async Task<IList<string>> GetRolesByUserIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? new List<string>() : await _userManager.GetRolesAsync(user);
    }
}