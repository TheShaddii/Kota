// Kota.Domain/Entities/User.cs
using System.ComponentModel.DataAnnotations;

namespace Kota.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(64)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(16)]
        public string Role { get; set; } = "user"; // "user" or "admin"

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public List<StockTransaction> StockTransactions { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();
    }
}