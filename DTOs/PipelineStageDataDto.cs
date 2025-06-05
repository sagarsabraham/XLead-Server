// DTOs/PipelineStageDataDto.cs
namespace XLead_Server.DTOs
{
    public class PipelineStageDataDto
    {
        public string StageName { get; set; }
        public decimal TotalAmount { get; set; } // Use decimal for currency
    }
}