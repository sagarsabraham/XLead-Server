using System;
using XLead_Server.DTOs;

namespace XLead_Server.DTOs
{
    public class DealReadDto
    {
        public long Id { get; set; }
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public long? AccountId { get; set; }
        public string? AccountName { get; set; }
        public long? RegionId { get; set; }
        public string? RegionName { get; set; }
        public long? DomainId { get; set; }
        public string? DomainName { get; set; }
        public long? RevenueTypeId { get; set; }
        public string? RevenueTypeName { get; set; }
        public long? DuId { get; set; }
        public string? DUName { get; set; }
        public long? CountryId { get; set; }
        public string? CountryName { get; set; }
        public string? Description { get; set; }
        public decimal? Probability { get; set; }
        public long? DealStageId { get; set; }
        public string? StageName { get; set; }
        public long ContactId { get; set; }
        public string? ContactName { get; set; }
        public long? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public bool IsHidden { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
