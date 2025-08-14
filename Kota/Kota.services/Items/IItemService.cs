// Kota.Services/Items/IItemService.cs
using Kota.Domain.Entities;
using Kota.Domain.DTOs;

namespace Kota.Services.Items
{
    public interface IItemService
    {
        Task<IEnumerable<ItemGridDto>> GetAllItemsAsync();
        Task<Item?> GetItemAsync(int id);
        Task<Item> CreateItemAsync(Item item, int userId);
        Task UpdateItemAsync(Item item, int userId);
        Task DeleteItemAsync(int id, int userId);
        Task AddStockAsync(int itemId, double quantity, string? note, int userId);
        Task RemoveStockAsync(int itemId, double quantity, string? note, int userId);
        Task BulkAddStockAsync(IEnumerable<int> itemIds, double quantity, int userId);
        Task BulkRemoveStockAsync(IEnumerable<int> itemIds, double quantity, int userId);
        Task BulkDeleteItemsAsync(IEnumerable<int> itemIds, int userId);
        Task<IEnumerable<ItemGridDto>> GetLowStockItemsAsync();
        Task<LocationDto?> GetItemLocationAsync(int itemId);
    }
}