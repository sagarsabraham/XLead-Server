namespace XLead_Server.Configuration
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public double Temperature { get; set; }
        public int MaxOutputTokens { get; set; }
    }
}