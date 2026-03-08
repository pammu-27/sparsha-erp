using System.ComponentModel.DataAnnotations;

namespace SparshaERP.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? CompanyName { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? ZipCode { get; set; }

        public string? Phone { get; set; }
    }
}
