// Kota.Domain/Entities/StorageUnit.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class StorageUnit
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int? AreaId { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(16)]
        public string Kind { get; set; } = string.Empty; // "container", "compartment"

        [Required, StringLength(32)]
        public string Type { get; set; } = string.Empty; // "cabinet", "shelf", etc.

        public Room? Room { get; set; }
        public Area? Area { get; set; }
        public List<Bin> Bins { get; set; } = new();
    }
}