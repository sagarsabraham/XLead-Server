// XLead_Server/DTOs/DealCreateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class DealCreateDto
    {
        [Required(ErrorMessage = "Deal Title/Name is required.")]
        [StringLength(255)]
        public string Title { get; set; } // Will map to Deal.DealName

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a non-negative value.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Contact Full Name is required.")]
        public string ContactFullName { get; set; }

        //[Required(ErrorMessage = "Salesperson Name is required.")]
        //public string SalespersonName { get; set; }

        // Nullable Foreign Keys
        public int? AccountId { get; set; }

        [Required(ErrorMessage = "Region is required.")]
        public int? RegionId { get; set; }

        public int? DomainId { get; set; }

        [Required(ErrorMessage = "Deal Stage is required.")]
        public long? DealStageId { get; set; }

        [Required(ErrorMessage = "Revenue Type is required.")]
        public int? RevenueTypeId { get; set; }

        [Required(ErrorMessage = "DU (Department) is required.")]
        public int? DuId { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public int? CountryId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public decimal? Probability { get; set; }

        [Required(ErrorMessage = "Starting Date is required.")]
        public DateTime? StartingDate { get; set; }

        [Required(ErrorMessage = "Closing Date is required.")]
        public DateTime? ClosingDate { get; set; }

        [Required]
        public long CreatedBy { get; set; } // User ID of the creator
    }
}