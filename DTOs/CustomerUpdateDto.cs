// DTOs/CustomerUpdateDto.cs

using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class CustomerUpdateDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer Name cannot be empty.")]
        [StringLength(255, MinimumLength = 2)]
        public string CustomerName { get; set; }

        // Use a more specific attribute for phone numbers if you have a standard format
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[\d\s\-\(\)\+]+$", ErrorMessage = "Phone number contains invalid characters.")]
        public string PhoneNo { get; set; }

        [StringLength(2083)]
        [Url(ErrorMessage = "Invalid website URL format.")]
        public string? Website { get; set; }

        public long IndustryVerticalId { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public long UpdatedBy { get; set; }
    }
}