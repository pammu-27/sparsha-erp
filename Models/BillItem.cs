namespace SparshaERP.Models
{
    public class BillItem
    {
        public int Id { get; set; }

        // relation
        public int BillId { get; set; }
        public Bill? Bill { get; set; }

        // item data
        public string Description { get; set; } = "";

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
    }
}
