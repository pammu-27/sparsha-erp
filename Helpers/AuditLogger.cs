using SparshaERP.Data;
using SparshaERP.Models;

namespace SparshaERP.Helpers
{
    public class AuditLogger
    {
        private readonly AppDbContext _db;

        public AuditLogger(AppDbContext db)
        {
            _db = db;
        }

        public void Log(int adminId, string adminName, string action, HttpContext http)
        {
            var log = new AdminAuditLog
            {
                AdminUserId = adminId,
                AdminName = adminName,
                Action = action,
                CreatedAtUtc = DateTime.UtcNow, // 🔥 FIXES PostgreSQL ERROR
                IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                UserAgent = http.Request.Headers["User-Agent"],
            };

            _db.AdminAuditLogs.Add(log);
            _db.SaveChanges();
        }
    }
}
