using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparshaERP.Data;
using SparshaERP.Helpers;
using SparshaERP.Models;

namespace SparshaERP.Controllers
{
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public ClientsController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                _auditLogger.Log(
                    HttpContext.Session.GetInt32("AdminId")!.Value,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Added new client '{client.Name}'",
                    HttpContext
                );
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            return View(client);
        }

        // EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(Client client)
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Updated client '{client.Name}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                _auditLogger.Log(
                    HttpContext.Session.GetInt32("AdminId")!.Value,
                    HttpContext.Session.GetString("AdminName")!,
                    $"Deleted client '{client.Name}'",
                    HttpContext
                );
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
