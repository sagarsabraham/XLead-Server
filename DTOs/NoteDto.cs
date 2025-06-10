using System;

namespace XLead_Server.DTOs
{
    public class NoteDto
    {
    public class NoteCreateDto
    {
        public string NoteText { get; set; }
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
    }

    public class NoteReadDto
    {
        public long Id { get; set; }
        public string NoteText { get; set; }
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class NoteUpdateDto
    {
        public string NoteText { get; set; }
        public long UpdatedBy { get; set; }
    }
}
}
