// Kota.Domain/Entities/Item.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Item
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(64)]
        public string? ManufacturerSku { get; set; }

        public double QtyOnHand { get; set; } = 0;
        public double MinQty { get; set; } = 0;
        public int BinId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long RowVersion { get; set; } = 0;

        public Bin? Bin { get; set; }
        public List<StockTransaction> StockTransactions { get; set; } = new();
    }
}