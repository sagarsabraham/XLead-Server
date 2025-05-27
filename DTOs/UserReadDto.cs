namespace XLead_Server.DTOs
{
    public class UserReadDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}