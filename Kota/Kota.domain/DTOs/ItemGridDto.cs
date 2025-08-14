// Kota.Domain/DTOs/ItemGridDto.cs
namespace Kota.Domain.DTOs
{
    public class ItemGridDto
    {
        public int Id { get; set; }
        public string? ManufacturerSku { get; set; }
        public string Description { get; set; } = string.Empty;
        public double QtyOnHand { get; set; }
        public double MinQty { get; set; }
        public string Site { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? Area { get; set; }
        public string StorageUnit { get; set; } = string.Empty;
        public string Bin { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public long RowVersion { get; set; }
        public bool IsLowStock => QtyOnHand < MinQty;
        public bool IsSelected { get; set; }
    }
}