using Microsoft.AspNetCore.Mvc;
using SparshaERP.Data;
using SparshaERP.Helpers;
using SparshaERP.Models;

namespace SparshaERP.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;
        private readonly EmailService _emailService;

        public SettingsController(
            AppDbContext context,
            AuditLogger auditLogger,
            EmailService emailService
        )
        {
            _context = context;
            _auditLogger = auditLogger;
            _emailService = emailService;
        }

        // ===================== SETTINGS HOME =====================
        public IActionResult Index()
        {
            int adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            if (adminId == 0)
                return RedirectToAction("Login", "Auth");

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == adminId);
            return View(admin);
        }

        // ===================== ADD ADMIN USERS (SUPER ADMIN ONLY) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdmin(
            string name,
            string email,
            string username,
            string password,
            string role
        )
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var superAdminId = HttpContext.Session.GetInt32("AdminId")!.Value;
            var superAdminName = HttpContext.Session.GetString("AdminName")!;

            var admin = new AdminUser
            {
                Name = name,
                Email = email,
                Username = username,
                PasswordHash = PasswordHelper.Hash(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = role
            };

            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            _auditLogger.Log(
                superAdminId,
                superAdminName,
                $"SuperAdmin added new admin '{username}'",
                HttpContext
            );

            return RedirectToAction("ManageAdmins");
        }

        // ===================== UPDATE OWN PROFILE (SUPER ADMIN ONLY) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string name, string email)
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            int adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == adminId);
            if (admin == null)
                return RedirectToAction("Login", "Auth");

            admin.Name = name;
            admin.Email = email;

            _context.SaveChanges();

            _auditLogger.Log(adminId, admin.Name, "Updated own profile", HttpContext);

            TempData["Success"] = "Profile updated successfully";
            return RedirectToAction("Index");
        }

        // ===================== MANAGE ADMINS (SUPER ADMIN ONLY) =====================
        public IActionResult ManageAdmins()
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var admins = _context.AdminUsers.ToList();
            return View(admins);
        }

        // ===================== UPDATE ADMIN PROFILE (SUPER ADMIN ONLY) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAdminProfile(int id, string name, string email, string username)
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var superAdminId = HttpContext.Session.GetInt32("AdminId")!.Value;
            var superAdminName = HttpContext.Session.GetString("AdminName")!;

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == id);
            if (admin == null)
                return RedirectToAction("ManageAdmins");

            admin.Name = name;
            admin.Email = email;
            admin.Username = username;

            _context.SaveChanges();

            _auditLogger.Log(
                superAdminId,
                superAdminName,
                $"SuperAdmin updated profile of admin '{username}'",
                HttpContext
            );

            return RedirectToAction("ManageAdmins");
        }

        // ===================== SET ADMIN PASSWORD (OPTION 2 – SUPER ADMIN ONLY) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAdminPassword(
            int adminId,
            string newPassword,
            string confirmPassword
        )
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match - password change failed";
                return RedirectToAction("ManageAdmins");
            }

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == adminId);
            if (admin == null)
                return RedirectToAction("ManageAdmins");

            if (admin.Username == "superadmin")
            {
                TempData["Error"] =
                    "Super Admin password cannot be changed here - use the 'Change Password' option in your profile instead";
                return RedirectToAction("ManageAdmins");
                _auditLogger.Log(
                    HttpContext.Session.GetInt32("AdminId")!.Value,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Attempted to change Super Admin password via Manage Admins page - blocked",
                    HttpContext
                );
            }

            admin.PasswordHash = PasswordHelper.Hash(newPassword);
            _context.SaveChanges();
            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"SuperAdmin set new password for admin '{admin.Username}'",
                HttpContext
            );
            var superAdminId = HttpContext.Session.GetInt32("AdminId")!.Value;
            var superAdminName = HttpContext.Session.GetString("AdminName")!;

            TempData["Success"] =
                "Admin password updated successfully - the admin can now login with the new password";
            _auditLogger.Log(
                superAdminId,
                superAdminName,
                $"SuperAdmin set new password for admin '{admin.Username}'",
                HttpContext
            );
            return RedirectToAction("ManageAdmins");
        }

        // ===================== ACTIVATE / DEACTIVATE ADMIN (SUPER ADMIN ONLY) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAdmin(int id)
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == id);
            if (admin == null)
                return RedirectToAction("ManageAdmins");

            admin.IsActive = !admin.IsActive;
            _context.SaveChanges();

            var superAdminId = HttpContext.Session.GetInt32("AdminId")!.Value;
            var superAdminName = HttpContext.Session.GetString("AdminName")!;

            _auditLogger.Log(
                superAdminId,
                superAdminName,
                $"SuperAdmin {(admin.IsActive ? "activated" : "deactivated")} admin '{admin.Username}'",
                HttpContext
            );

            return RedirectToAction("ManageAdmins");
        }

        // for future: add delete admin functionality with proper safeguards (e.g. cannot delete self, cannot delete super admin, confirmation prompt, etc.)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAdminRole(int id, string role)
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == id);
            if (admin == null)
                return RedirectToAction("ManageAdmins");

            admin.Role = role;

            _context.SaveChanges();

            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"SuperAdmin changed role of '{admin.Username}' to '{role}'",
                HttpContext
            );

            TempData["Success"] = "Admin role updated successfully";
            return RedirectToAction("ManageAdmins");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAdmin(int id)
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == id);
            if (admin == null)
                return RedirectToAction("ManageAdmins");

            // ❌ Prevent deleting yourself
            if (admin.Id == HttpContext.Session.GetInt32("AdminId"))
            {
                TempData["Error"] = "You cannot delete your own account";
                return RedirectToAction("ManageAdmins");
            }

            // ❌ Prevent deleting SuperAdmin
            if (admin.Role == "SuperAdmin")
            {
                TempData["Error"] = "SuperAdmin cannot be deleted";
                return RedirectToAction("ManageAdmins");
            }

            _context.AdminUsers.Remove(admin);
            _context.SaveChanges();

            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"SuperAdmin deleted admin '{admin.Username}'",
                HttpContext
            );

            TempData["Success"] = "Admin deleted successfully";
            return RedirectToAction("ManageAdmins");
        }

        // ===================== SUPER ADMIN CHANGE OWN PASSWORD =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeSuperAdminPassword(
            string currentPassword,
            string newPassword,
            string confirmPassword,
            string otp
        )
        {
            // 🔐 Only Super Admin allowed
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            int adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            if (adminId == 0)
                return RedirectToAction("Login", "Auth");

            // 🔁 Password confirmation check
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match - password change failed";
                _auditLogger.Log(
                    adminId,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Failed password change attempt due to password mismatch - RE-AUTH ENFORCED",
                    HttpContext
                );
                return RedirectToAction("Index");
            }

            // 🔐 Password strength enforcement
            if (!PasswordPolicy.IsStrong(newPassword, out var policyError))
            {
                TempData["Error"] = policyError;
                return RedirectToAction("Index");
            }

            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == adminId);
            if (admin == null)
                return RedirectToAction("Login", "Auth");

            // 🔍 Verify current password
            if (!PasswordHelper.Verify(currentPassword, admin.PasswordHash))
            {
                TempData["Error"] = "Current password is incorrect - password change failed";
                _auditLogger.Log(
                    adminId,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Failed password change attempt due to incorrect current password - RE-AUTH ENFORCED",
                    HttpContext
                );
                return RedirectToAction("Index");
            }

            // 🔐 OTP verification (RE-AUTH)
            var otpEntry = _context.AdminOtps.FirstOrDefault(o =>
                o.AdminId == adminId && o.Otp == otp && o.ExpiresAt > DateTime.UtcNow
            );

            if (otpEntry == null)
            {
                TempData["Error"] = "Invalid or expired OTP - password change failed";
                _auditLogger.Log(
                    adminId,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Failed password change attempt due to invalid/expired OTP - RE-AUTH ENFORCED",
                    HttpContext
                );
                return RedirectToAction("Index");
            }

            // 🔐 Update password
            admin.PasswordHash = PasswordHelper.Hash(newPassword);

            // 🔥 Remove used OTP
            _context.AdminOtps.Remove(otpEntry);

            _context.SaveChanges();

            // 🧾 Audit log
            _auditLogger.Log(
                adminId,
                admin.Name,
                "SuperAdmin changed own password with OTP verification - RE-AUTH ENFORCED ",
                HttpContext
            );
            _auditLogger.Log(
                adminId,
                admin.Name,
                "SuperAdmin changed own password successfully",
                HttpContext
            );

            // 📧 Security alert email
            _emailService.Send(
                admin.Email,
                "Security Alert: Password Changed for Your Super Admin Account",
                $@"
                Your Super Admin password was changed successfully .

                Time (IST): {DateTime.UtcNow}
                
                IP Address: {HttpContext.Connection.RemoteIpAddress}

                If this was NOT you, contact support immediately ."
            );
            _auditLogger.Log(
                adminId,
                admin.Name,
                "SuperAdmin password changed successfully",
                HttpContext
            );

            // 🔒 Force re-login
            HttpContext.Session.Clear();

            TempData["Success"] = "Password changed successfully. Please login again.";
            // remove all previous OTPs for safety
            _context.AdminOtps.RemoveRange(_context.AdminOtps.Where(o => o.AdminId == adminId));
            return RedirectToAction("Login", "Auth");
            _auditLogger.Log(
                adminId,
                admin.Name,
                "SuperAdmin password changed successfully - RE-AUTH ENFORCED, ALL PREVIOUS OTPs INVALIDATED",
                HttpContext
            );
        }

        //========================SEND EMAIL OTP FOR PASSWORD CHANGE (SUPER ADMIN ONLY)=========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendSuperAdminOtp(
            string currentPassword,
            string newPassword,
            string confirmPassword
        )
        {
            if (HttpContext.Session.GetString("Role") != "SuperAdmin")
                return Unauthorized();

            int adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var admin = _context.AdminUsers.FirstOrDefault(x => x.Id == adminId);
            if (admin == null)
                return Unauthorized();

            // 🔍 1. Verify current password
            if (!PasswordHelper.Verify(currentPassword, admin.PasswordHash))
            {
                return BadRequest("Current password is incorrect - OTP not sent");
            }

            // 🔁 2. Confirm password match
            if (newPassword != confirmPassword)
            {
                return BadRequest("New password and confirm password do not match - OTP not sent");
            }

            // 🔐 3. Password strength check
            if (!PasswordPolicy.IsStrong(newPassword, out var policyError))
            {
                return BadRequest(policyError);
            }

            // ✅ ALL CHECKS PASSED → SEND OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // remove old OTPs
            _context.AdminOtps.RemoveRange(_context.AdminOtps.Where(o => o.AdminId == adminId));

            _context.AdminOtps.Add(
                new AdminOtp
                {
                    AdminId = adminId,
                    Otp = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                }
            );

            _context.SaveChanges();

            _emailService.Send(
                admin.Email,
                " OTP for Password Change SparshaERP Settings Page ",
                $"Your OTP is {otp}. Valid for 5 minutes only"
            );
            _auditLogger.Log(
                adminId,
                admin.Name,
                "Sent OTP for SuperAdmin password change",
                HttpContext
            );
            return Ok($"OTP sent successfully to your registered email address {admin.Email}");
        }
    }
}
