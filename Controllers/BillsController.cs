//Controllers\BillsController.cs
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SparshaERP.Data;
using SparshaERP.Helpers;
using SparshaERP.Models;

namespace SparshaERP.Controllers
{
    public class BillsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public BillsController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        //for manual bill creation
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Bill bill, List<BillItem> items)
        {
            bill.InvoiceNo = await GenerateInvoiceNo();

            // ✅ FIX DATE
            bill.Date = DateTime.UtcNow;

            if (bill.DueDate.HasValue)
            {
                bill.DueDate = DateTime.SpecifyKind(bill.DueDate.Value, DateTimeKind.Utc);
            }

            bill.SubTotal = items.Sum(x => x.Amount);
            bill.TotalAmount =
                bill.SubTotal + bill.TaxAmount + bill.TransportCharge + bill.OtherCharge;

            bill.Items = items;

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // =========================================
        // LIST
        // =========================================
        public async Task<IActionResult> Index()
        {
            var bills = await _context
                .Bills.Include(x => x.Client)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(bills);
        }

        // =========================================
        // EDIT (VIEW BILL)
        // =========================================
        public async Task<IActionResult> Edit(int id)
        {
            var bill = await _context
                .Bills.Include(x => x.Client)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bill == null)
                return NotFound();

            return View(bill);
        }

        // =========================================
        // UPDATE PAYMENT STATUS
        // =========================================
        [HttpPost]
        public async Task<IActionResult> Edit(Bill bill)
        {
            var db = await _context.Bills.FindAsync(bill.Id);

            if (db == null)
                return NotFound();

            db.IsPaid = bill.IsPaid;

            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin updated payment status of invoice '{bill.InvoiceNo}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // DELETE
        // =========================================
        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills.FindAsync(id);

            if (bill != null)
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();

                var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
                var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
                _auditLogger.Log(
                    adminId,
                    adminName,
                    $"Admin deleted invoice '{bill.InvoiceNo}' for client '{bill.Client?.Name}'",
                    HttpContext
                );
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SetDueDate(int id, DateTime dueDate)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
                return NotFound();

            bill.DueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin set due date of invoice '{bill.InvoiceNo}' to {dueDate:dd-MM-yyyy}",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // PREMIUM PDF
        // =========================================
        public async Task<IActionResult> Print(int id)
        {
            var bill = await _context
                .Bills.Include(b => b.Client)
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
                return NotFound();

            var stream = new MemoryStream();

            string blue = "#0B4F9C";

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.png");

            Document
                .Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(25);

                        page.Content()
                            .Column(col =>
                            {
                                col.Spacing(10);

                                // ================= HEADER =================
                                col.Item()
                                    .Row(row =>
                                    {
                                        // LOGO
                                        row.ConstantItem(70).Height(70).Image(logoPath);

                                        // COMPANY DETAILS
                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        "SPARSHA ALUMINIUM FABRICATION & INTERIORS"
                                                    )
                                                    .Bold()
                                                    .FontSize(16);

                                                c.Item().Text("Moodbidri, Karnataka");
                                                c.Item().Text("Phone: 9108829511");
                                                c.Item().Text("Email: sparsha@example.com");
                                            });

                                        // INVOICE BOX
                                        row.ConstantItem(220)
                                            .Border(1)
                                            .Padding(6)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .AlignCenter()
                                                    .Text("INVOICE")
                                                    .Bold()
                                                    .FontSize(18);

                                                c.Item().Text($"Invoice #: {bill.InvoiceNo}");
                                                c.Item().Text($"Date: {bill.Date:dd-MM-yyyy}");
                                            });
                                    });

                                col.Item().LineHorizontal(1);

                                // ================= CUSTOMER BAR =================
                                col.Item()
                                    .Background(blue)
                                    .Padding(5)
                                    .Text("CUSTOMER DETAILS")
                                    .FontColor("#FFF")
                                    .Bold();

                                // CUSTOMER BOX
                                col.Item()
                                    .Border(1)
                                    .Padding(6)
                                    .Column(c =>
                                    {
                                        c.Item().Text(bill.Client?.Name ?? "");
                                        c.Item().Text(bill.Client?.Address ?? "");
                                        c.Item().Text($"ZIP: {bill.Client?.ZipCode}");
                                        c.Item().Text($"Phone: {bill.Client?.Phone}");
                                    });

                                // ================= ITEMS TABLE =================
                                col.Item().PaddingTop(10);

                                col.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(5); // desc
                                            columns.RelativeColumn(1); // qty
                                            columns.RelativeColumn(2); // rate
                                            columns.RelativeColumn(2); // amount
                                        });

                                        // HEADER
                                        table.Header(header =>
                                        {
                                            header
                                                .Cell()
                                                .Background(blue)
                                                .Padding(6)
                                                .Text("DESCRIPTION")
                                                .FontColor("#FFF")
                                                .Bold();

                                            header
                                                .Cell()
                                                .Background(blue)
                                                .Padding(6)
                                                .Text("QTY")
                                                .FontColor("#FFF")
                                                .Bold();

                                            header
                                                .Cell()
                                                .Background(blue)
                                                .Padding(6)
                                                .Text("RATE")
                                                .FontColor("#FFF")
                                                .Bold();

                                            header
                                                .Cell()
                                                .Background(blue)
                                                .Padding(6)
                                                .Text("AMOUNT")
                                                .FontColor("#FFF")
                                                .Bold();
                                        });

                                        // ROWS
                                        foreach (var item in bill.Items)
                                        {
                                            table
                                                .Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(item.Description);
                                            table
                                                .Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(item.Qty.ToString());
                                            table
                                                .Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text($"₹ {item.Rate:0.00}");
                                            table
                                                .Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text($"₹ {item.Amount:0.00}");
                                        }
                                    });

                                // ================= TOTAL BOX =================
                                col.Item()
                                    .AlignRight()
                                    .Width(280)
                                    .Border(1)
                                    .Padding(8)
                                    .Column(c =>
                                    {
                                        void Row(string label, decimal value, bool bold = false)
                                        {
                                            c.Item()
                                                .Row(r =>
                                                {
                                                    r.RelativeItem().Text(label);
                                                    r.ConstantItem(120)
                                                        .AlignRight()
                                                        .Text($"₹ {value:0.00}")
                                                        .Bold();
                                                });
                                        }

                                        Row("Subtotal", bill.SubTotal);
                                        Row("Tax", bill.TaxAmount);
                                        Row("Transport", bill.TransportCharge);
                                        Row("Other Charges", bill.OtherCharge);

                                        c.Item().LineHorizontal(1);

                                        Row("GRAND TOTAL", bill.TotalAmount, true);

                                        c.Item().LineHorizontal(1);

                                        Row("Paid", bill.PaidAmount);
                                        Row("Balance", bill.TotalAmount - bill.PaidAmount, true);
                                    });

                                // ================= FOOTER =================
                                col.Item()
                                    .AlignCenter()
                                    .PaddingTop(20)
                                    .Text("Thank you for your business!")
                                    .Bold();
                            });
                    });
                })
                .GeneratePdf(stream);

            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin printed invoice '{bill.InvoiceNo}' for client '{bill.Client?.Name}'",
                HttpContext
            );
            return File(stream.ToArray(), "application/pdf", $"Invoice_{bill.InvoiceNo}.pdf");
        }

        //bill status controller
        private void UpdateBillStatus(Bill bill)
        {
            if (bill.PaidAmount >= bill.TotalAmount)
                bill.Status = "Paid";
            else if (bill.PaidAmount > 0)
                bill.Status = "Partial";
            else if (DateTime.Today > bill.DueDate)
                bill.Status = "Overdue";
            else
                bill.Status = "Unpaid";
        }

        //mark paid or Partial payment controller
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.Bills.FindAsync(id);

            bill.PaidAmount = bill.TotalAmount;

            UpdateBillStatus(bill);

            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin marked invoice '{bill.InvoiceNo}' as paid for client '{bill.Client?.Name}'",
                HttpContext
            );

            return RedirectToAction(nameof(Index));
        }

        //Partial payment
        [HttpPost]
        public async Task<IActionResult> AddPayment(int id, decimal amount)
        {
            var bill = await _context.Bills.FindAsync(id);

            bill.PaidAmount += amount;

            UpdateBillStatus(bill);

            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin added payment of ₹ {amount:0.00} to invoice '{bill.InvoiceNo}' for client '{bill.Client?.Name}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        //bills invoice number
        private async Task<string> GenerateInvoiceNo()
        {
            var year = DateTime.Now.Year;

            var last = await _context
                .Bills.Where(x => x.InvoiceNo.Contains(year.ToString()))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = int.Parse(last.InvoiceNo.Split('-').Last());
                next = num + 1;
            }

            return $"INV-{year}-{next:0000}";
        }

        //export to excel
        public async Task<IActionResult> ExportExcel(int id)
        {
            var bill = await _context
                .Bills.Include(b => b.Client)
                .Include(b => b.Items)
                .FirstAsync(b => b.Id == id);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Invoice");

            ws.Cell("A1").Value = "Invoice No";
            ws.Cell("B1").Value = bill.InvoiceNo;

            ws.Cell("A3").Value = "Description";
            ws.Cell("B3").Value = "Qty";
            ws.Cell("C3").Value = "Rate";
            ws.Cell("D3").Value = "Amount";

            int r = 4;

            foreach (var i in bill.Items)
            {
                ws.Cell(r, 1).Value = i.Description;
                ws.Cell(r, 2).Value = i.Qty;
                ws.Cell(r, 3).Value = i.Rate;
                ws.Cell(r, 4).Value = i.Amount;
                r++;
            }

            ws.Cell(r + 1, 3).Value = "Total";
            ws.Cell(r + 1, 4).Value = bill.TotalAmount;

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            var adminId = HttpContext.Session.GetInt32("AdminId") ?? 0;
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Unknown";
            _auditLogger.Log(
                adminId,
                adminName,
                $"Admin exported invoice '{bill.InvoiceNo}' to Excel for client '{bill.Client?.Name}'",
                HttpContext
            );
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Invoice_{bill.InvoiceNo}.xlsx"
            );
        }
    }
}
