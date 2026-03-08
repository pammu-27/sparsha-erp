namespace SparshaERP.Models
{
    public class AdminOtp
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string Otp { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
