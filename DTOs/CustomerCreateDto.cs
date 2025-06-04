// XLead_Server/DTOs/CompanyCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class CustomerCreateDto
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string CustomerName { get; set; }

        [StringLength(2083)]
        public string? Website { get; set; }

        [StringLength(50)]
        public string? CustomerPhoneNumber { get; set; }

        [Required]
        public long CreatedBy { get; set; }
    }
}