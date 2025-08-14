// Kota.Domain/Entities/Area.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Area
    {
        public int Id { get; set; }
        public int RoomId { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public Room? Room { get; set; }
    }
}