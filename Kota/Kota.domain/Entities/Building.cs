// Kota.Domain/Entities/Building.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Building
    {
        public int Id { get; set; }
        public int SiteId { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public Site? Site { get; set; }
        public List<Room> Rooms { get; set; } = new();
    }
}