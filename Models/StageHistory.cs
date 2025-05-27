namespace XLead_Server.Models
{
    public class StageHistory
    {
        public long Id { get; set; }
        public long StageId { get; set; }
       
        //public long DealId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        //public Deal Deal { get; set; }
    }
}
