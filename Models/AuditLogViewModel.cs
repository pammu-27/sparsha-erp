namespace SparshaERP.Models.ViewModels
{
    public class AuditLogViewModel
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
