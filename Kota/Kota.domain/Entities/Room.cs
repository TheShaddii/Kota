// Kota.Domain/Entities/Room.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public Building? Building { get; set; }
        public List<Area> Areas { get; set; } = new();
        public List<StorageUnit> StorageUnits { get; set; } = new();
    }
}