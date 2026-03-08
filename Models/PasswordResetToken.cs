using DocumentFormat.OpenXml.Office2010.Excel;

public class PasswordResetToken
{
    public int id { get; set; }
    public int AdminUserId { get; set; }
    public string Token { get; set; } = "";
    public DateTime Expiry { get; set; }
}
