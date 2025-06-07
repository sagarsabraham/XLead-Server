using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class DealEditDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string ContactFullName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactDesignation { get; set; }
        public long? ServiceId { get; set; }
        public long? AccountId { get; set; }
        [Required]
        public long RegionId { get; set; }
        public long? DomainId { get; set; }
        [Required]
        public long DealStageId { get; set; }
        [Required]
        public long RevenueTypeId { get; set; }
        [Required]
        public long DuId { get; set; }
        [Required]
        public long CountryId { get; set; }
        public string? Description { get; set; }
        public decimal? Probability { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
