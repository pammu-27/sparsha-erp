using System.ComponentModel.DataAnnotations;

namespace SparshaERP.Models
{
    public class Bill
    {
        public int Id { get; set; }

        // ================= BASIC INFO =================

        [Required]
        public string InvoiceNo { get; set; } = "";

        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        // ================= CLIENT =================

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // ================= RELATION =================

        // generated from quotation
        public int QuotationId { get; set; }

        // ================= ITEMS =================

        public List<BillItem> Items { get; set; } = new();

        //⭐ NEW payments
        public decimal PaidAmount { get; set; } = 0;

        // ⭐ NEW status
        public string Status { get; set; } = "Unpaid";

        // ================= TOTALS =================

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TransportCharge { get; set; }

        public decimal OtherCharge { get; set; }

        public decimal TotalAmount { get; set; }

        // ================= PAYMENT =================

        public bool IsPaid { get; set; } = false;
    }
}
