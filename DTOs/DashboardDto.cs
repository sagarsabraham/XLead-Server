// DTOs/DashboardDtos.cs
namespace XLead_Server.DTOs
{
    public class DashboardMetricItemDto
    {
        public string Value { get; set; } = "0";
        public double PercentageChange { get; set; } = 0;
        public bool IsPositiveTrend { get; set; } = true; 
    }

    public class DashboardMetricsDto
    {
        public DashboardMetricItemDto OpenPipelines { get; set; } = new DashboardMetricItemDto();
        public DashboardMetricItemDto PipelinesWon { get; set; } = new DashboardMetricItemDto();
        public DashboardMetricItemDto PipelinesLost { get; set; } = new DashboardMetricItemDto();
        public DashboardMetricItemDto RevenueWon { get; set; } = new DashboardMetricItemDto();
    }
}