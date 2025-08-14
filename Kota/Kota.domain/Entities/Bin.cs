// Kota.Domain/Entities/Bin.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Bin
    {
        public int Id { get; set; }
        public int StorageUnitId { get; set; }

        [Required, StringLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(16)]
        public string Kind { get; set; } = string.Empty; // "bin", "slot"

        public StorageUnit? StorageUnit { get; set; }
        public List<Item> Items { get; set; } = new();
    }
}
