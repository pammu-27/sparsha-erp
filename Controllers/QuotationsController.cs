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
    public class QuotationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public QuotationsController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        // =========================================================
        // INDEX
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var quotations = await _context
                .Quotations.Include(q => q.Client)
                .Include(q => q.Items)
                .OrderByDescending(q => q.Id)
                .ToListAsync();

            return View(quotations);
        }

        // =========================================================
        // CREATE GET
        // =========================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View(new Quotation());
        }

        // =========================================================
        // CREATE POST
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            Quotation quotation,
            List<string> description,
            List<decimal> qty,
            List<decimal> rate
        )
        {
            quotation.Date = DateTime.UtcNow;

            // 🔹 Auto Quote Number
            var count = await _context.Quotations.CountAsync() + 1;
            quotation.QuoteNo = $"Q-{count:0000}";

            quotation.Items = new List<QuotationItem>();

            decimal subtotal = 0;

            // 🔹 Add items
            for (int i = 0; i < description.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(description[i]))
                    continue;

                decimal amount = qty[i] * rate[i];

                quotation.Items.Add(
                    new QuotationItem
                    {
                        Description = description[i],
                        Qty = qty[i],
                        Rate = rate[i],
                        Amount = amount,
                    }
                );

                subtotal += amount;
            }

            // 🔹 Charges
            decimal tax = quotation.ApplyTax ? subtotal * quotation.TaxPercent / 100 : 0;

            decimal transport = quotation.ApplyTransport ? quotation.TransportCharge : 0;

            decimal other = quotation.ApplyOther ? quotation.OtherCharge : 0;

            quotation.TotalAmount = subtotal + tax + transport + other;

            _context.Add(quotation);
            await _context.SaveChangesAsync();
            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Created new quotation '{quotation.QuoteNo}' for client '{quotation.Client?.Name}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT GET
        // =========================================================
        public async Task<IActionResult> Edit(int id)
        {
            var quotation = await _context
                .Quotations.Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            ViewBag.Clients = await _context.Clients.ToListAsync();

            return View(quotation);
        }

        // =========================================================
        // EDIT POST
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Edit(
            int id,
            Quotation form,
            List<string> description,
            List<decimal> qty,
            List<decimal> rate
        )
        {
            var quotation = await _context
                .Quotations.Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            // 🔹 Update simple fields
            quotation.ClientId = form.ClientId;
            quotation.ApplyTax = form.ApplyTax;
            quotation.TaxPercent = form.TaxPercent;
            quotation.ApplyTransport = form.ApplyTransport;
            quotation.TransportCharge = form.TransportCharge;
            quotation.ApplyOther = form.ApplyOther;
            quotation.OtherCharge = form.OtherCharge;

            // 🔹 Remove old items
            _context.QuotationItems.RemoveRange(quotation.Items);

            quotation.Items = new List<QuotationItem>();

            decimal subtotal = 0;

            // 🔹 Add new items
            for (int i = 0; i < description.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(description[i]))
                    continue;

                decimal amount = qty[i] * rate[i];

                quotation.Items.Add(
                    new QuotationItem
                    {
                        Description = description[i],
                        Qty = qty[i],
                        Rate = rate[i],
                        Amount = amount,
                    }
                );

                subtotal += amount;
            }

            // 🔹 Charges
            decimal tax = quotation.ApplyTax ? subtotal * quotation.TaxPercent / 100 : 0;

            decimal transport = quotation.ApplyTransport ? quotation.TransportCharge : 0;

            decimal other = quotation.ApplyOther ? quotation.OtherCharge : 0;

            quotation.TotalAmount = subtotal + tax + transport + other;

            await _context.SaveChangesAsync();
            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Edited quotation '{quotation.QuoteNo}' for client '{quotation.Client?.Name}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE
        // =========================================================
        public async Task<IActionResult> Delete(int id)
        {
            var quotation = await _context
                .Quotations.Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            _context.QuotationItems.RemoveRange(quotation.Items);
            _context.Quotations.Remove(quotation);

            await _context.SaveChangesAsync();

            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Deleted quotation '{quotation.QuoteNo}' for client '{quotation.Client?.Name}'",
                HttpContext
            );
            return RedirectToAction(nameof(Index));
        }

        //pdf print
        public async Task<IActionResult> Print(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var quotation = await _context
                .Quotations.Include(q => q.Client)
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.png");

            // ================= CALCULATIONS =================
            decimal subtotal = quotation.Items.Sum(x => x.Amount);

            decimal tax = quotation.ApplyTax ? subtotal * quotation.TaxPercent / 100 : 0;

            decimal transport = quotation.ApplyTransport ? quotation.TransportCharge : 0;

            decimal other = quotation.ApplyOther ? quotation.OtherCharge : 0;

            decimal total = subtotal + tax + transport + other;

            string blue = "#0B4F9C";
            string lightGray = "#F4F6F8";

            var stream = new MemoryStream();

            Document
                .Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);

                        // ================= DEFAULT FONT =================
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content()
                            .Column(col =>
                            {
                                col.Spacing(14);

                                // =================================================
                                // HEADER
                                // =================================================
                                col.Item()
                                    .Row(row =>
                                    {
                                        row.ConstantItem(65).Height(65).Image(logoPath);

                                        row.RelativeItem()
                                            .PaddingLeft(10)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        "SPARSHA ALUMINIUM FABRICATION & INTERIORS"
                                                    )
                                                    .FontSize(18)
                                                    .Bold();

                                                c.Item().Text("Moodbidri");
                                                c.Item().Text("Phone : 9108829511");
                                            });

                                        row.ConstantItem(230)
                                            .Border(1)
                                            .Padding(10)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .AlignCenter()
                                                    .Text("QUOTATION")
                                                    .FontSize(16)
                                                    .Bold();

                                                c.Item()
                                                    .Text($"Date : {quotation.Date:dd-MM-yyyy}");
                                                c.Item().Text($"Quote # : {quotation.QuoteNo}");
                                                c.Item()
                                                    .Text($"Customer ID : {quotation.ClientId}");
                                                c.Item()
                                                    .Text(
                                                        $"Valid Until : {quotation.Date.AddDays(15):dd-MM-yyyy}"
                                                    );
                                            });
                                    });

                                col.Item().LineHorizontal(1);

                                // =================================================
                                // CUSTOMER SECTION
                                // =================================================
                                SectionHeader(col, "CUSTOMER", blue);

                                col.Item()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Spacing(2);
                                        c.Item().Text(quotation.Client?.Name ?? "").Bold();
                                        c.Item().Text(quotation.Client?.Address ?? "");
                                        c.Item().Text($"ZIP : {quotation.Client?.ZipCode ?? ""}");
                                        c.Item().Text($"Phone : {quotation.Client?.Phone ?? ""}");
                                    });

                                // =================================================
                                // ITEMS TABLE
                                // =================================================
                                // SectionHeader(col, "DESCRIPTION", blue);

                                col.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(6);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(2);
                                        });

                                        // HEADER ROW
                                        table.Header(header =>
                                        {
                                            HeaderCell(header, "DESCRIPTION", blue);
                                            HeaderCell(header, "TAXED", blue, true);
                                            HeaderCell(header, "AMOUNT", blue, false, true);
                                        });

                                        int i = 0;

                                        foreach (var item in quotation.Items)
                                        {
                                            var bg = i % 2 == 0 ? lightGray : "#FFFFFF";

                                            BodyCell(table, item.Description, bg);
                                            BodyCell(
                                                table,
                                                quotation.ApplyTax ? "X" : "-",
                                                bg,
                                                true
                                            );
                                            BodyCell(
                                                table,
                                                $"₹ {item.Amount:0.00}",
                                                bg,
                                                false,
                                                true
                                            );

                                            i++;
                                        }
                                    });

                                // =================================================
                                // TOTAL CARD (RIGHT SIDE)
                                // =================================================
                                col.Item()
                                    .AlignRight()
                                    .Width(270)
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        RowTotal(c, "Subtotal", subtotal);
                                        RowTotal(c, "Tax", tax);
                                        RowTotal(c, "Transport", transport);
                                        RowTotal(c, "Other", other);

                                        c.Item().LineHorizontal(1);

                                        RowTotal(c, "TOTAL", total, true);
                                    });

                                // =================================================
                                // TERMS
                                // =================================================
                                SectionHeader(col, "TERMS & CONDITIONS", blue);

                                col.Item()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text(
                                                "1. Customer will be billed after indicating acceptance of this quote"
                                            );
                                        c.Item()
                                            .Text(
                                                "2. Payment due prior to delivery of service and goods"
                                            );
                                        c.Item().Text("3. Please WhatsApp signed copy");
                                    });

                                // =================================================
                                // SIGNATURE + THANK YOU
                                // =================================================
                                col.Item()
                                    .PaddingTop(35)
                                    .Row(r =>
                                    {
                                        r.RelativeItem();

                                        r.ConstantItem(200)
                                            .Column(c =>
                                            {
                                                c.Item().LineHorizontal(1);
                                                c.Item().AlignCenter().Text("Authorized Signature");
                                            });
                                    });
                                col.Item()
                                    .AlignCenter()
                                    .Text(
                                        "If you have any questions about this price quote, please contact"
                                    )
                                    .FontSize(11);
                                col.Item().AlignCenter().Text("[Deepak, 9108829511]").FontSize(10);

                                col.Item()
                                    .AlignCenter()
                                    .Text("Thank You For Your Business!")
                                    .Bold()
                                    .FontSize(12);
                            });
                    });
                })
                .GeneratePdf(stream);
            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Generated PDF for quotation '{quotation.QuoteNo}' for client '{quotation.Client?.Name}'",
                HttpContext
            );
            return File(stream.ToArray(), "application/pdf", $"Quotation_{quotation.QuoteNo}.pdf");

            // =================================================
            // HELPER FUNCTIONS
            // =================================================

            static void SectionHeader(ColumnDescriptor col, string text, string color)
            {
                col.Item().Background(color).Padding(6).Text(text).FontColor("#FFFFFF").Bold();
            }

            static void HeaderCell(
                TableCellDescriptor cell,
                string text,
                string color,
                bool center = false,
                bool right = false
            )
            {
                var c = cell.Cell()
                    .Background(color)
                    .Padding(6)
                    .Text(text)
                    .FontColor("#FFF")
                    .Bold();

                if (center)
                    c.AlignCenter();
                if (right)
                    c.AlignRight();
            }

            static void BodyCell(
                TableDescriptor table,
                string text,
                string bg,
                bool center = false,
                bool right = false
            )
            {
                var c = table.Cell().Background(bg).BorderBottom(1).Padding(5).Text(text);

                if (center)
                    c.AlignCenter();
                if (right)
                    c.AlignRight();
            }

            static void RowTotal(ColumnDescriptor c, string label, decimal value, bool bold = false)
            {
                c.Item()
                    .Row(r =>
                    {
                        var left = r.RelativeItem().Text(label);
                        var right = r.ConstantItem(120).AlignRight().Text($"₹ {value:0.00}");

                        if (bold)
                        {
                            left.Bold();
                            right.Bold();
                        }
                    });
            }
        }

        //bill generator
        public async Task<IActionResult> GenerateBill(int id)
        {
            var quotation = await _context
                .Quotations.Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
                return NotFound();

            var count = await _context.Bills.CountAsync() + 1;

            var bill = new Bill
            {
                InvoiceNo = $"INV-{count:0000}",
                ClientId = quotation.ClientId,
                Date = DateTime.UtcNow,

                SubTotal = quotation.SubTotal,
                TaxAmount = quotation.TaxAmount,
                TransportCharge = quotation.TransportCharge,
                OtherCharge = quotation.OtherCharge,
                TotalAmount = quotation.TotalAmount,

                Status = "Pending",

                Items = quotation
                    .Items.Select(i => new BillItem
                    {
                        Description = i.Description,
                        Qty = i.Qty,
                        Rate = i.Rate,
                        Amount = i.Amount,
                    })
                    .ToList(),
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            _auditLogger.Log(
                HttpContext.Session.GetInt32("AdminId")!.Value,
                HttpContext.Session.GetString("AdminName")!,
                $"Generated bill '{bill.InvoiceNo}' from quotation '{quotation.QuoteNo}' for client '{quotation.Client?.Name}'",
                HttpContext
            );

            return RedirectToAction("Index", "Bills");
        }
    }
}
