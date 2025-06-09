// DTOs/ContactUpdateDto.cs

using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class ContactUpdateDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name cannot be empty.")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(100)]
        // Regex asserts that the string is not composed ONLY of digits.
        [RegularExpression(@"^(?!^\d+$).*", ErrorMessage = "Designation cannot consist only of numbers.")]
        public string Designation { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Email cannot be empty.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[\d\s\-\(\)\+]+$", ErrorMessage = "Phone number contains invalid characters.")]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public long UpdatedBy { get; set; }
    }
}