// Kota.Domain/Entities/AuditLog.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(32)]
        public string EntityType { get; set; } = string.Empty;

        public int EntityId { get; set; }

        [Required, StringLength(32)]
        public string Action { get; set; } = string.Empty;

        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }

        public User? User { get; set; }
    }
}