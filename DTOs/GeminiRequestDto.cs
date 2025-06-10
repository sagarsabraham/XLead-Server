// XLead_Server/DTOs/GeminiDto.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace XLead_Server.DTOs
{
    // --- Request DTOs ---

    public class GeminiRequestDto
    {
        [JsonPropertyName("contents")]
        public List<GeminiContentDto> Contents { get; set; }

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfigDto GenerationConfig { get; set; }
    }

    public class GeminiContentDto
    {
        [JsonPropertyName("parts")]
        public List<GeminiPartDto> Parts { get; set; }
    }

    public class GeminiPartDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GeminiGenerationConfigDto
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }

        // You could add other settings like topK, topP here if needed
    }

    // --- Response DTOs ---

    public class GeminiResponseDto
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidateDto> Candidates { get; set; }

        [JsonPropertyName("promptFeedback")]
        public GeminiPromptFeedbackDto PromptFeedback { get; set; }
    }

    public class GeminiCandidateDto
    {
        [JsonPropertyName("content")]
        public GeminiContentDto Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("safetyRatings")]
        public List<GeminiSafetyRatingDto> SafetyRatings { get; set; }
    }

    public class GeminiSafetyRatingDto
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("probability")]
        public string Probability { get; set; }
    }

    public class GeminiPromptFeedbackDto
    {
        [JsonPropertyName("safetyRatings")]
        public List<GeminiSafetyRatingDto> SafetyRatings { get; set; }
    }
}