using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparshaERP.Data;

namespace SparshaERP.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly AppDbContext _db;

        public AuditLogsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // 🔒 SUPER ADMIN ONLY
            if (
                !string.Equals(
                    HttpContext.Session.GetString("Role"),
                    "SuperAdmin",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return Unauthorized();
            }

            var logs = _db.AdminAuditLogs.OrderByDescending(x => x.CreatedAtUtc).ToList();

            return View(logs);
        }

        // CSV EXPORT
        public IActionResult ExportCsv()
        {
            var logs = _db.AdminAuditLogs.OrderByDescending(x => x.CreatedAtUtc).ToList();

            var csv = "Id,AdminUserId,Action,IP,DateTime\n";

            foreach (var l in logs)
            {
                csv +=
                    $"{l.Id},{l.AdminUserId},\"{l.Action}\",{l.IpAddress},{l.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "AuditLogs.csv");
        }
    }
}
