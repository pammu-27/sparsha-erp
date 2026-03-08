namespace SparshaERP.Models
{
    public class DashboardViewModel
    {
        // KPI cards
        public int TotalQuotations { get; set; }
        public int TotalBills { get; set; }
        public decimal TotalRevenue { get; set; }

        // Overdue
        public int OverdueBillsCount { get; set; }
        public decimal OverdueAmount { get; set; }

        // Monthly chart
        public List<string> Months { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();

        // Recent activity
        public List<Quotation> RecentQuotations { get; set; } = new();
    }
}
