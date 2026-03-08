using System;

namespace SparshaERP.Models
{
    public class AdminAuditLog
    {
        public int Id { get; set; }

        public int AdminUserId { get; set; }
        public string AdminName { get; set; } = "";

        public string Action { get; set; } = "";

        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";

        // MUST BE UTC for PostgreSQL
        public DateTime CreatedAtUtc { get; set; }
    }
}
