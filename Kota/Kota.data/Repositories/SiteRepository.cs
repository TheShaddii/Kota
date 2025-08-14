using Kota.Domain.Entities;

namespace Kota.Data.Repositories
{
    public interface ISiteRepository
    {
        Task<IEnumerable<Site>> GetAllAsync();
        Task<Site?> GetByIdAsync(int id);
        Task<Site> CreateAsync(Site site);
        Task UpdateAsync(Site site);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string code, int? excludeId = null);
    }

    public class SiteRepository : BaseRepository, ISiteRepository
    {
        public SiteRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<Site>> GetAllAsync()
        {
            return await QueryAsync<Site>("SELECT id, code, name FROM sites ORDER BY name");
        }

        public async Task<Site?> GetByIdAsync(int id)
        {
            return await QuerySingleOrDefaultAsync<Site>("SELECT id, code, name FROM sites WHERE id = ?", new { id });
        }

        public async Task<Site> CreateAsync(Site site)
        {
            // Use simpler INSERT for Access
            var sql = "INSERT INTO sites (code, name) VALUES (?, ?)";
            await ExecuteAsync(sql, new { site.Code, site.Name });

            // Get the last inserted ID using @@IDENTITY
            var id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            site.Id = id;
            return site;
        }

        public async Task UpdateAsync(Site site)
        {
            await ExecuteAsync("UPDATE sites SET code = ?, name = ? WHERE id = ?",
                new { site.Code, site.Name, site.Id });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteAsync("DELETE FROM sites WHERE id = ?", new { id });
        }

        public async Task<bool> ExistsAsync(string code, int? excludeId = null)
        {
            var sql = "SELECT COUNT(*) FROM sites WHERE code = ?";
            object param = new { code };

            if (excludeId.HasValue)
            {
                sql += " AND id <> ?";
                param = new { code, id = excludeId.Value };
            }

            var count = await ExecuteScalarAsync<int>(sql, param);
            return count > 0;
        }
    }
}