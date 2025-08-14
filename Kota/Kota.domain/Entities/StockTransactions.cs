// Kota.Domain/Entities/StockTransaction.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public double QtyDelta { get; set; }

        [Required, StringLength(32)]
        public string ReasonCode { get; set; } = string.Empty;

        public string? Note { get; set; }

        public User? User { get; set; }
        public Item? Item { get; set; }
    }
}