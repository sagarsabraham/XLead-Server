namespace XLead_Server.Configuration
{
    public class OpenAISettings
    {
        public const string SectionName = "OpenAISettings";

        public string ApiKey { get; set; }
        public string ApiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string Model { get; set; } = "gpt-3.5-turbo";
        public double Temperature { get; set; } = 0.2;
        public int MaxTokens { get; set; } = 250;
    }
}