// Kota.Services/Items/ItemService.cs
using Kota.Data.Repositories;
using Kota.Domain.Entities;
using Kota.Domain.DTOs;
using Kota.Domain.Constants;
using Newtonsoft.Json;

namespace Kota.Services.Items
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly IAuditLogRepository _auditRepository;

        public ItemService(
            IItemRepository itemRepository,
            IStockTransactionRepository stockTransactionRepository,
            IAuditLogRepository auditRepository)
        {
            _itemRepository = itemRepository;
            _stockTransactionRepository = stockTransactionRepository;
            _auditRepository = auditRepository;
        }

        public async Task<IEnumerable<ItemGridDto>> GetAllItemsAsync()
        {
            return await _itemRepository.GetAllForGridAsync();
        }

        public async Task<Item?> GetItemAsync(int id)
        {
            return await _itemRepository.GetByIdAsync(id);
        }

        public async Task<Item> CreateItemAsync(Item item, int userId)
        {
            var createdItem = await _itemRepository.CreateAsync(item);

            // Create initial stock transaction if starting quantity > 0
            if (item.QtyOnHand > 0)
            {
                await _stockTransactionRepository.CreateAsync(new StockTransaction
                {
                    UserId = userId,
                    ItemId = createdItem.Id,
                    QtyDelta = item.QtyOnHand,
                    ReasonCode = ReasonCodes.InitialLoad,
                    Note = "Initial inventory load"
                });
            }

            // Log item creation
            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                EntityType = "item",
                EntityId = createdItem.Id,
                Action = AuditActions.Create,
                AfterJson = JsonConvert.SerializeObject(createdItem)
            });

            return createdItem;
        }

        public async Task UpdateItemAsync(Item item, int userId)
        {
            var existing = await _itemRepository.GetByIdAsync(item.Id);
            if (existing == null)
                throw new InvalidOperationException("Item not found");

            var before = JsonConvert.SerializeObject(existing);

            await _itemRepository.UpdateAsync(item);

            var after = JsonConvert.SerializeObject(item);

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                EntityType = "item",
                EntityId = item.Id,
                Action = AuditActions.Update,
                BeforeJson = before,
                AfterJson = after
            });
        }

        public async Task DeleteItemAsync(int id, int userId)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
                throw new InvalidOperationException("Item not found");

            var before = JsonConvert.SerializeObject(item);

            // Create stock transaction to zero out inventory
            if (item.QtyOnHand > 0)
            {
                await _stockTransactionRepository.CreateAsync(new StockTransaction
                {
                    UserId = userId,
                    ItemId = id,
                    QtyDelta = -item.QtyOnHand,
                    ReasonCode = ReasonCodes.DeleteItem,
                    Note = "Item deleted"
                });
            }

            await _itemRepository.DeleteAsync(id);

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                EntityType = "item",
                EntityId = id,
                Action = AuditActions.Delete,
                BeforeJson = before
            });
        }

        public async Task AddStockAsync(int itemId, double quantity, string? note, int userId)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null)
                throw new InvalidOperationException("Item not found");

            var before = JsonConvert.SerializeObject(new { item.QtyOnHand });

            item.QtyOnHand += quantity;
            await _itemRepository.UpdateAsync(item);

            await _stockTransactionRepository.CreateAsync(new StockTransaction
            {
                UserId = userId,
                ItemId = itemId,
                QtyDelta = quantity,
                ReasonCode = ReasonCodes.Add,
                Note = note
            });

            var after = JsonConvert.SerializeObject(new { item.QtyOnHand });

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                EntityType = "item",
                EntityId = itemId,
                Action = AuditActions.Update,
                BeforeJson = before,
                AfterJson = after
            });
        }

        public async Task RemoveStockAsync(int itemId, double quantity, string? note, int userId)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null)
                throw new InvalidOperationException("Item not found");

            if (item.QtyOnHand < quantity)
                throw new InvalidOperationException("Insufficient stock");

            var before = JsonConvert.SerializeObject(new { item.QtyOnHand });

            item.QtyOnHand -= quantity;
            await _itemRepository.UpdateAsync(item);

            await _stockTransactionRepository.CreateAsync(new StockTransaction
            {
                UserId = userId,
                ItemId = itemId,
                QtyDelta = -quantity,
                ReasonCode = ReasonCodes.Remove,
                Note = note
            });

            var after = JsonConvert.SerializeObject(new { item.QtyOnHand });

            await _auditRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                EntityType = "item",
                EntityId = itemId,
                Action = AuditActions.Update,
                BeforeJson = before,
                AfterJson = after
            });
        }

        public async Task BulkAddStockAsync(IEnumerable<int> itemIds, double quantity, int userId)
        {
            foreach (var itemId in itemIds)
            {
                await AddStockAsync(itemId, quantity, $"Bulk add of {quantity}", userId);
            }
        }

        public async Task BulkRemoveStockAsync(IEnumerable<int> itemIds, double quantity, int userId)
        {
            foreach (var itemId in itemIds)
            {
                await RemoveStockAsync(itemId, quantity, $"Bulk remove of {quantity}", userId);
            }
        }

        public async Task BulkDeleteItemsAsync(IEnumerable<int> itemIds, int userId)
        {
            foreach (var itemId in itemIds)
            {
                await DeleteItemAsync(itemId, userId);
            }
        }

        public async Task<IEnumerable<ItemGridDto>> GetLowStockItemsAsync()
        {
            return await _itemRepository.GetLowStockItemsAsync();
        }

        public async Task<LocationDto?> GetItemLocationAsync(int itemId)
        {
            return await _itemRepository.GetItemLocationAsync(itemId);
        }
    }
}