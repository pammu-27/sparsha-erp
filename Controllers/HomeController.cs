using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparshaERP.Data;
using SparshaERP.Models;

namespace SparshaERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            // ================= TOTAL COUNTS =================

            vm.TotalBills = await _context.Bills.CountAsync();

            vm.TotalQuotations = await _context.Quotations.CountAsync();

            // ================= TOTAL REVENUE =================

            vm.TotalRevenue = await _context.Bills.SumAsync(x => x.TotalAmount);

            // ================= OVERDUE BILLS =================

            var overdue = await _context
                .Bills.Where(x =>
                    x.DueDate != null && x.DueDate < DateTime.UtcNow && x.PaidAmount < x.TotalAmount
                )
                .ToListAsync();

            vm.OverdueBillsCount = overdue.Count;

            vm.OverdueAmount = overdue.Sum(x => x.TotalAmount - x.PaidAmount);

            // ================= RECENT QUOTATIONS =================

            vm.RecentQuotations = await _context
                .Quotations.Include(x => x.Client)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            // ================= MONTHLY REVENUE =================

            var months = Enumerable
                .Range(0, 6)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            vm.Months = months.Select(x => x.ToString("MMM yyyy")).ToList();

            vm.MonthlyRevenue = new List<decimal>();

            foreach (var m in months)
            {
                var revenue =
                    await _context
                        .Bills.Where(x => x.Date.Month == m.Month && x.Date.Year == m.Year)
                        .SumAsync(x => (decimal?)x.TotalAmount)
                    ?? 0;

                vm.MonthlyRevenue.Add(revenue);
            }

            return View(vm);
        }
    }
}
