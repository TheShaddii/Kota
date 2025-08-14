// Kota.Services/Authentication/IAuthService.cs
using Kota.Domain.Entities;

namespace Kota.Services.Authentication
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> CreateUserAsync(string username, string password, string role, int currentUserId);
        Task UpdateUserAsync(User user, int currentUserId);
        Task DeactivateUserAsync(int userId, int currentUserId);
        Task ChangePasswordAsync(int userId, string newPassword, int currentUserId);
        Task<bool> IsDefaultPasswordAsync(string username);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}