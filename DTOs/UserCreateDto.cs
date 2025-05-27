namespace XLead_Server.DTOs
{
    public class UserCreateDto
    {
        public string Name { get; set; }
        public long? CreatedBy { get; set; }
        public long? AssignedTo { get; set; }
    }
}