using System.ComponentModel.DataAnnotations;

namespace SparshaERP.Models;

public class Quotation
{
    public int Id { get; set; }

    public string QuoteNo { get; set; } = "";

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public List<QuotationItem> Items { get; set; } = new();

    // totals
    public decimal SubTotal { get; set; }

    public bool ApplyTax { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }

    public bool ApplyTransport { get; set; }
    public decimal TransportCharge { get; set; }

    public bool ApplyOther { get; set; }
    public decimal OtherCharge { get; set; }

    public decimal TotalAmount { get; set; }
}
