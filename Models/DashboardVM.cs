namespace SparshaERP.ViewModels
{
    public class DashboardVM
    {
        public int TotalClients { get; set; }
        public int TotalQuotations { get; set; }
        public int TotalBills { get; set; }

        public int PendingBills { get; set; }
        public int OverdueBills { get; set; }

        public decimal TotalBilledAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalBalanceAmount { get; set; }
    }
}
