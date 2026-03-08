using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparshaERP.Data;
using SparshaERP.Helpers;
using SparshaERP.Models;

namespace SparshaERP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public AuthController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        // ================= LOGIN =================

        /// <summary>
        /// Returns a view for the login page.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            var admin = _context.AdminUsers.FirstOrDefault(x =>
                x.Username == username && x.IsActive
            );

            if (admin == null || !PasswordHelper.Verify(password, admin.PasswordHash))
            {
                _auditLogger.Log(
                    0,
                    "Unknown",
                    $"Failed login attempt for username '{username}'",
                    HttpContext
                );
                ViewBag.Error = "Invalid username or password";
                return View();
            }
            HttpContext.Session.SetString("IsAdmin", "true");
            HttpContext.Session.SetInt32("AdminId", admin.Id);
            HttpContext.Session.SetString("AdminName", admin.Name);

            // 🔥 THIS LINE IS REQUIRED
            HttpContext.Session.SetString("Role", admin.Role);
            _auditLogger.Log(admin.Id, admin.Name, $"Admin logged in", HttpContext);
            return RedirectToAction("Index", "Home");
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminId != null)
            {
                _auditLogger.Log(
                    adminId.Value,
                    adminName ?? "Unknown",
                    $"Admin logged out",
                    HttpContext
                );
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= FORGOT PASSWORD =================
    }
}
