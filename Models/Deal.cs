//namespace XLead_Server.Models
//{
//    public class Deal
//    {
//        public long Id { get; set; }
//        public string DealName { get; set; }
//        public decimal DealAmount { get; set; }
//        public long AccountId { get; set; }
//        public long RegionId { get; set; }
//        public long DomainId { get; set; }
//        public long DealStageId { get; set; }
//        public long RevenueTypeId { get; set; }
//        public long DuId { get; set; }
//        public long CountryId { get; set; }
//        public long ServiceLineId { get; set; }
//        public string Description { get; set; } 
//        public decimal Probability { get; set; } 
//        public long ContactId { get; set; } 
//        public DateTime StartingDate { get; set; } 
//        public DateTime ClosingDate { get; set; } 
//        public long CreatedBy { get; set; } 
//        public DateTime CreatedAt { get; set; } = DateTime.Now; 
//        public DateTime UpdatedAt { get; set; }

//        public Account account { get; set; }
//        public Region region { get; set; }
//        public Domain domain { get; set; }
//        public DealStage dealStage { get; set; }
//        public RevenueType revenueType { get; set; }
//        public DU du { get; set; }
//        public Country country { get; set; }
//        public Contact contact { get; set; }
//        public ServiceLine serviceLine { get; set; }

//        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>();
//        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
//    }
//}


namespace XLead_Server.Models
{
    public class Deal
    {
        public long Id { get; set; }
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public long AccountId { get; set; }
        public long RegionId { get; set; }
        public long DomainId { get; set; }
        
     

        public long  DealStageId { get; set; }
        public DealStage DealStage { get; set; }  // Optional navigation property


        public long RevenueTypeId { get; set; }
        public long DuId { get; set; }
        public long CountryId { get; set; }
        public long ServiceLineId { get; set; }
        public string Description { get; set; }
        public decimal Probability { get; set; }
        public long ContactId { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public bool? IsHidden { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User Creator { get; set; }
        public Account account { get; set; }
        public Region region { get; set; }
        public Domain domain { get; set; }
        public DealStage dealStage { get; set; }
        public RevenueType revenueType { get; set; }
        public DU du { get; set; }
        public Country country { get; set; }
        public Contact contact { get; set; }
        public ServiceLine serviceLine { get; set; }

        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<Note> Notes { get; set; }
    }

}