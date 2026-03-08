namespace SparshaERP.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "Admin";
    }
}
