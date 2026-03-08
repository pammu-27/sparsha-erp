namespace SparshaERP.Models;

public class QuotationItem
{
    public int Id { get; set; }

    public int QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public string Description { get; set; } = "";

    public decimal Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}
