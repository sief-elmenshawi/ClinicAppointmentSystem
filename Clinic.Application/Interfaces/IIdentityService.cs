public interface IIdentityService
{
    Task<(bool Succeeded, string? UserId, string? Error)> CreateUserAsync(
        string email, string password, string role);

    Task<(bool Succeeded, string? UserId, IList<string>? Roles, string? Error)> ValidateCredentialsAsync(
        string email, string password);

    Task<string?> GetUserEmailAsync(string userId);
    Task<IList<string>> GetRolesByUserIdAsync(string userId);

}