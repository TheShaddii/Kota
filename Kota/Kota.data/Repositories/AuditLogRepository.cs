// Kota.Data/Repositories/AuditLogRepository.cs
using Kota.Domain.Entities;

namespace Kota.Data.Repositories
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    }

    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            auditLog.Timestamp = DateTime.Now;

            var sql = @"INSERT INTO audit_log (ts, user_id, entity_type, entity_id, action, before_json, after_json) 
                       VALUES (?, ?, ?, ?, ?, ?, ?)";

            await ExecuteAsync(sql, new
            {
                auditLog.Timestamp,
                auditLog.UserId,
                auditLog.EntityType,
                auditLog.EntityId,
                auditLog.Action,
                auditLog.BeforeJson,
                auditLog.AfterJson
            });

            auditLog.Id = await ExecuteScalarAsync<int>("SELECT @@IDENTITY");
            return auditLog;
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
        {
            return await QueryAsync<AuditLog>(
                $"SELECT TOP {limit} id, ts, user_id, entity_type, entity_id, action, before_json, after_json FROM audit_log ORDER BY ts DESC");
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId)
        {
            return await QueryAsync<AuditLog>(
                "SELECT id, ts, user_id, entity_type, entity_id, action, before_json, after_json FROM audit_log WHERE entity_type = ? AND entity_id = ? ORDER BY ts DESC",
                new { entityType, entityId });
        }
    }
}