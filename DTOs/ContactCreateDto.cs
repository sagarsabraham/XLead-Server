using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class ContactCreateDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [Required]
        [StringLength(255)]
        public string CustomerName { get; set; }

        public long CustomerId { get; set; }

        [Required]
        public long CreatedBy { get; set; }
    }
}