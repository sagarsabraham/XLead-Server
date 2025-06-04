// XLead_Server/DTOs/ContactCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class ContactCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [Required]
        public long CustomerId { get; set; } // Used when creating contact for existing company

        [Required]
        public long CreatedBy { get; set; }

        // This is used by CompanyContactController but might not be needed
        // if CompanyId is always reliably set.
        // For DealRepository logic where company might be created, CompanyId is more direct.
        public string? CustomerName { get; set; }
    }
}