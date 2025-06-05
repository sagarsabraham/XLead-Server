namespace XLead_Server.Models
{
    public class Note
    {
        public long Id { get; set; }
        public long DealId { get; set; }
        public string NoteText { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
