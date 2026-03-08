public class AuditLogVM
{
    public int Id { get; set; }
    public string AdminName { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Ip { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
