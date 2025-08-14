// Kota.Data/Repositories/UserRepository.cs
using Kota.Domain.Entities;

namespace Kota.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string username, int? excludeId = null);
    }

    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await QuerySingleOrDefaultAsync<User>(
                "SELECT id, username, password_hash, role, is_active, created_at FROM users WHERE username = ? AND is_active = TRUE",
                new { username });
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await QuerySingleOrDefaultAsync<User>(
                "SELECT id, username, password_hash, role, is_active, created_at FROM users WHERE id = ?",
                new { id });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await QueryAsync<User>(
                "SELECT id, username, password_hash, role, is_active, created_at FROM users ORDER BY username");
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.Now;

            var sql = "INSERT INTO users (username, password_hash, role, is_active, created_at) VALUES (?, ?, ?, ?, ?)";
            await ExecuteAsync(sql, new
            {
                user.Username,
                user.PasswordHash,
                user.Role,
                user.IsActive,
                user.CreatedAt
            });

            user.Id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            var sql = "UPDATE users SET username = ?, password_hash = ?, role = ?, is_active = ? WHERE id = ?";
            await ExecuteAsync(sql, new
            {
                user.Username,
                user.PasswordHash,
                user.Role,
                user.IsActive,
                user.Id
            });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteAsync("UPDATE users SET is_active = FALSE WHERE id = ?", new { id });
        }

        public async Task<bool> ExistsAsync(string username, int? excludeId = null)
        {
            var sql = "SELECT COUNT(*) FROM users WHERE username = ?";
            object param = new { username };

            if (excludeId.HasValue)
            {
                sql += " AND id <> ?";
                param = new { username, id = excludeId.Value };
            }

            var count = await ExecuteScalarAsync<int>(sql, param);
            return count > 0;
        }
    }
}