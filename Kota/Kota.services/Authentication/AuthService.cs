// Kota.Services/Authentication/AuthService.cs
using Kota.Data.Repositories;
using Kota.Domain.Entities;
using Kota.Domain.Constants;
using Newtonsoft.Json;

namespace Kota.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditRepository;

        public AuthService(IUserRepository userRepository, IAuditLogRepository auditRepository)
        {
            _userRepository = userRepository;
            _auditRepository = auditRepository;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Log successful login
            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = user.Id,
                EntityType = "user",
                EntityId = user.Id,
                Action = AuditActions.Login,
                AfterJson = JsonConvert.SerializeObject(new { Username = user.Username, LoginTime = DateTime.Now })
            });

            return user;
        }

        public async Task<User> CreateUserAsync(string username, string password, string role, int currentUserId)
        {
            if (await _userRepository.ExistsAsync(username))
                throw new InvalidOperationException("Username already exists");

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role,
                IsActive = true
            };

            user = await _userRepository.CreateAsync(user);

            // Log user creation
            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = currentUserId,
                EntityType = "user",
                EntityId = user.Id,
                Action = AuditActions.Create,
                AfterJson = JsonConvert.SerializeObject(new { user.Username, user.Role, user.IsActive })
            });

            return user;
        }

        public async Task UpdateUserAsync(User user, int currentUserId)
        {
            var existing = await _userRepository.GetByIdAsync(user.Id);
            if (existing == null)
                throw new InvalidOperationException("User not found");

            var before = JsonConvert.SerializeObject(new { existing.Username, existing.Role, existing.IsActive });

            await _userRepository.UpdateAsync(user);

            var after = JsonConvert.SerializeObject(new { user.Username, user.Role, user.IsActive });

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = currentUserId,
                EntityType = "user",
                EntityId = user.Id,
                Action = AuditActions.Update,
                BeforeJson = before,
                AfterJson = after
            });
        }

        public async Task DeactivateUserAsync(int userId, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var before = JsonConvert.SerializeObject(new { user.Username, user.Role, IsActive = user.IsActive });

            await _userRepository.DeleteAsync(userId);

            var after = JsonConvert.SerializeObject(new { user.Username, user.Role, IsActive = false });

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = currentUserId,
                EntityType = "user",
                EntityId = userId,
                Action = AuditActions.Delete,
                BeforeJson = before,
                AfterJson = after
            });
        }

        public async Task ChangePasswordAsync(int userId, string newPassword, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = currentUserId,
                EntityType = "user",
                EntityId = userId,
                Action = AuditActions.PasswordReset,
                AfterJson = JsonConvert.SerializeObject(new { PasswordChanged = DateTime.Now })
            });
        }

        public async Task<bool> IsDefaultPasswordAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return false;

            return VerifyPassword("ChangeMe!123", user.PasswordHash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}