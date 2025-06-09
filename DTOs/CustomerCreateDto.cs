using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class CustomerCreateDto
    {
        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string CustomerName { get; set; }

        [StringLength(2083)]
        [Url(ErrorMessage = "Invalid website URL format.")]
        public string? Website { get; set; }

        [Required]
        [StringLength(50)]
        // Regex allows numbers, spaces, hyphens, parentheses, and a leading '+'
        [RegularExpression(@"^[\d\s\-\(\)\+]+$", ErrorMessage = "Phone number contains invalid characters.")]
        public string CustomerPhoneNumber { get; set; }

        public long? IndustryVerticalId { get; set; }

        [Required]
        public long CreatedBy { get; set; }
    }
}