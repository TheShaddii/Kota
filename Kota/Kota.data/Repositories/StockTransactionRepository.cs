// Kota.Data/Repositories/StockTransactionRepository.cs
using Kota.Domain.Entities;

namespace Kota.Data.Repositories
{
    public interface IStockTransactionRepository
    {
        Task<StockTransaction> CreateAsync(StockTransaction transaction);
        Task<IEnumerable<StockTransaction>> GetByItemIdAsync(int itemId);
        Task<IEnumerable<StockTransaction>> GetRecentAsync(int limit = 100);
    }

    public class StockTransactionRepository : BaseRepository, IStockTransactionRepository
    {
        public StockTransactionRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<StockTransaction> CreateAsync(StockTransaction transaction)
        {
            transaction.Timestamp = DateTime.Now;

            var sql = @"INSERT INTO stock_transactions (ts, user_id, item_id, qty_delta, reason_code, note) 
                       VALUES (?, ?, ?, ?, ?, ?)";

            await ExecuteAsync(sql, new
            {
                transaction.Timestamp,
                transaction.UserId,
                transaction.ItemId,
                transaction.QtyDelta,
                transaction.ReasonCode,
                transaction.Note
            });

            transaction.Id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            return transaction;
        }

        public async Task<IEnumerable<StockTransaction>> GetByItemIdAsync(int itemId)
        {
            return await QueryAsync<StockTransaction>(
                "SELECT id, ts, user_id, item_id, qty_delta, reason_code, note FROM stock_transactions WHERE item_id = ? ORDER BY ts DESC",
                new { itemId });
        }

        public async Task<IEnumerable<StockTransaction>> GetRecentAsync(int limit = 100)
        {
            return await QueryAsync<StockTransaction>(
                $"SELECT TOP {limit} id, ts, user_id, item_id, qty_delta, reason_code, note FROM stock_transactions ORDER BY ts DESC");
        }
    }
}