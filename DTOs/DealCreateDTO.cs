using System;
using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class DealCreateDto
    {
        [Required(ErrorMessage = "Deal Title/Name is required.")]
        [StringLength(255)]
        public string Title { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a non-negative value.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Contact Full Name is required.")]
        public string ContactFullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? ContactEmail { get; set; }

        public string? ContactPhoneNumber { get; set; }

        public string? ContactDesignation { get; set; }

        public long? ServiceId { get; set; }

        public long? AccountId { get; set; }

        [Required(ErrorMessage = "Region is required.")]
        public long? RegionId { get; set; }

        public long? DomainId { get; set; }

        [Required(ErrorMessage = "Deal Stage is required.")]
        public long? DealStageId { get; set; }

        [Required(ErrorMessage = "Revenue Type is required.")]
        public long? RevenueTypeId { get; set; }

        [Required(ErrorMessage = "DU (Department) is required.")]
        public long? DuId { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public long? CountryId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public decimal? Probability { get; set; }

        [Required(ErrorMessage = "Starting Date is required.")]
        public DateTime? StartingDate { get; set; }

        [Required(ErrorMessage = "Closing Date is required.")]
        public DateTime? ClosingDate { get; set; }

        [Required]
        public long CreatedBy { get; set; }
      
        public Dictionary<string, object>? CustomFields { get; set; }

    }
}