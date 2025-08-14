// Kota.Data/Repositories/ItemRepository.cs
using Kota.Domain.Entities;
using Kota.Domain.DTOs;

namespace Kota.Data.Repositories
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemGridDto>> GetAllForGridAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<Item> CreateAsync(Item item);
        Task UpdateAsync(Item item);
        Task DeleteAsync(int id);
        Task<LocationDto?> GetItemLocationAsync(int itemId);
        Task<IEnumerable<ItemGridDto>> GetLowStockItemsAsync();
    }

    public class ItemRepository : BaseRepository, IItemRepository
    {
        public ItemRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<ItemGridDto>> GetAllForGridAsync()
        {
            var sql = @"
                SELECT 
                    i.id,
                    i.manufacturer_sku,
                    i.description,
                    i.qty_on_hand,
                    i.min_qty,
                    s.name AS site,
                    b.name AS building,
                    r.name AS room,
                    a.name AS area,
                    su.code AS storage_unit,
                    bn.code AS bin,
                    i.notes,
                    i.row_version
                FROM items i
                INNER JOIN bins bn ON i.bin_id = bn.id
                INNER JOIN storage_units su ON bn.storage_unit_id = su.id
                INNER JOIN rooms r ON su.room_id = r.id
                LEFT JOIN areas a ON su.area_id = a.id
                INNER JOIN buildings b ON r.building_id = b.id
                INNER JOIN sites s ON b.site_id = s.id
                ORDER BY i.description";

            return await QueryAsync<ItemGridDto>(sql);
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await QuerySingleOrDefaultAsync<Item>(@"
                SELECT id, description, manufacturer_sku, qty_on_hand, min_qty, 
                       bin_id, notes, created_at, updated_at, row_version 
                FROM items WHERE id = ?", new { id });
        }

        public async Task<Item> CreateAsync(Item item)
        {
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
            item.RowVersion = 1;

            var sql = @"INSERT INTO items (description, manufacturer_sku, qty_on_hand, min_qty, 
                                         bin_id, notes, created_at, updated_at, row_version) 
                       VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

            await ExecuteAsync(sql, new
            {
                item.Description,
                item.ManufacturerSku,
                item.QtyOnHand,
                item.MinQty,
                item.BinId,
                item.Notes,
                item.CreatedAt,
                item.UpdatedAt,
                item.RowVersion
            });

            item.Id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            return item;
        }

        public async Task UpdateAsync(Item item)
        {
            item.UpdatedAt = DateTime.Now;
            item.RowVersion++;

            var sql = @"UPDATE items SET 
                          description = ?, manufacturer_sku = ?, qty_on_hand = ?, min_qty = ?,
                          bin_id = ?, notes = ?, updated_at = ?, row_version = ?
                       WHERE id = ? AND row_version = ?";

            var affected = await ExecuteAsync(sql, new
            {
                item.Description,
                item.ManufacturerSku,
                item.QtyOnHand,
                item.MinQty,
                item.BinId,
                item.Notes,
                item.UpdatedAt,
                item.RowVersion,
                item.Id,
                RowVersionCheck = item.RowVersion - 1
            });

            if (affected == 0)
                throw new InvalidOperationException("Item was modified by another user. Please reload and try again.");
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteAsync("DELETE FROM items WHERE id = ?", new { id });
        }

        public async Task<LocationDto?> GetItemLocationAsync(int itemId)
        {
            var sql = @"
                SELECT 
                    s.id AS SiteId, s.name AS SiteName,
                    b.id AS BuildingId, b.name AS BuildingName,
                    r.id AS RoomId, r.name AS RoomName,
                    a.id AS AreaId, a.name AS AreaName,
                    su.id AS StorageUnitId, su.code AS StorageUnitName,
                    bn.id AS BinId, bn.code AS BinName
                FROM items i
                INNER JOIN bins bn ON i.bin_id = bn.id
                INNER JOIN storage_units su ON bn.storage_unit_id = su.id
                INNER JOIN rooms r ON su.room_id = r.id
                LEFT JOIN areas a ON su.area_id = a.id
                INNER JOIN buildings b ON r.building_id = b.id
                INNER JOIN sites s ON b.site_id = s.id
                WHERE i.id = ?";

            return await QuerySingleOrDefaultAsync<LocationDto>(sql, new { itemId });
        }

        public async Task<IEnumerable<ItemGridDto>> GetLowStockItemsAsync()
        {
            var sql = @"
                SELECT 
                    i.id,
                    i.manufacturer_sku,
                    i.description,
                    i.qty_on_hand,
                    i.min_qty,
                    s.name AS site,
                    b.name AS building,
                    r.name AS room,
                    a.name AS area,
                    su.code AS storage_unit,
                    bn.code AS bin,
                    i.notes,
                    i.row_version
                FROM items i
                INNER JOIN bins bn ON i.bin_id = bn.id
                INNER JOIN storage_units su ON bn.storage_unit_id = su.id
                INNER JOIN rooms r ON su.room_id = r.id
                LEFT JOIN areas a ON su.area_id = a.id
                INNER JOIN buildings b ON r.building_id = b.id
                INNER JOIN sites s ON b.site_id = s.id
                WHERE i.qty_on_hand < i.min_qty
                ORDER BY i.description";

            return await QueryAsync<ItemGridDto>(sql);
        }
    }
}