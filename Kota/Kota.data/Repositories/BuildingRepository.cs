using Kota.Domain.Entities;

namespace Kota.Data.Repositories
{
    public interface IBuildingRepository
    {
        Task<IEnumerable<Building>> GetAllAsync();
        Task<IEnumerable<Building>> GetBySiteIdAsync(int siteId);
        Task<Building?> GetByIdAsync(int id);
        Task<Building> CreateAsync(Building building);
        Task UpdateAsync(Building building);
        Task DeleteAsync(int id);
    }

    public class BuildingRepository : BaseRepository, IBuildingRepository
    {
        public BuildingRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<Building>> GetAllAsync()
        {
            return await QueryAsync<Building>("SELECT id, site_id, code, name FROM buildings ORDER BY name");
        }

        public async Task<IEnumerable<Building>> GetBySiteIdAsync(int siteId)
        {
            return await QueryAsync<Building>("SELECT id, site_id, code, name FROM buildings WHERE site_id = ? ORDER BY name", new { siteId });
        }

        public async Task<Building?> GetByIdAsync(int id)
        {
            return await QuerySingleOrDefaultAsync<Building>("SELECT id, site_id, code, name FROM buildings WHERE id = ?", new { id });
        }

        public async Task<Building> CreateAsync(Building building)
        {
            var sql = "INSERT INTO buildings (site_id, code, name) VALUES (?, ?, ?)";
            await ExecuteAsync(sql, new { building.SiteId, building.Code, building.Name });

            var id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            building.Id = id;
            return building;
        }

        public async Task UpdateAsync(Building building)
        {
            await ExecuteAsync("UPDATE buildings SET site_id = ?, code = ?, name = ? WHERE id = ?",
                new { building.SiteId, building.Code, building.Name, building.Id });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteAsync("DELETE FROM buildings WHERE id = ?", new { id });
        }
    }
}