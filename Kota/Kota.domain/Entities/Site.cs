// Kota.Domain/Entities/Site.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class Site
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public List<Building> Buildings { get; set; } = new();
    }
}