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

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[\d\s\-\(\)\+]+$", ErrorMessage = "Phone number contains invalid characters.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        // Regex asserts that the string is not composed ONLY of digits.
        [RegularExpression(@"^(?!^\d+$).*", ErrorMessage = "Designation cannot consist only of numbers.")]
        public string Designation { get; set; }

        [Required]
        [StringLength(255)]
        public string CustomerName { get; set; }

        public long CustomerId { get; set; }

        [Required]
        public long CreatedBy { get; set; }
    }
}