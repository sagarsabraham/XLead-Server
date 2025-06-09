//using System.Text.Json.Serialization;
//using System.Collections.Generic;

//namespace XLead_Server.DTOs
//{
//    public class OpenAiChatCompletionRequestDto
//    {
//        [JsonPropertyName("model")]
//        public string Model { get; set; }

//        [JsonPropertyName("messages")]
//        public List<OpenAiMessageDto> Messages { get; set; }

//        [JsonPropertyName("temperature")]
//        public double Temperature { get; set; }

//        [JsonPropertyName("max_tokens")]
//        public int MaxTokens { get; set; }
//    }

//    public class OpenAiChatCompletionResponseDto
//    {
//        [JsonPropertyName("id")]
//        public string Id { get; set; }

//        [JsonPropertyName("object")]
//        public string ObjectType { get; set; }

//        [JsonPropertyName("created")]
//        public long Created { get; set; }

//        [JsonPropertyName("model")]
//        public string Model { get; set; }

//        [JsonPropertyName("choices")]
//        public List<OpenAiChoiceDto> Choices { get; set; }

//        [JsonPropertyName("usage")]
//        public OpenAiUsageDto Usage { get; set; }
//    }

//    public class OpenAiChoiceDto
//    {
//        [JsonPropertyName("index")]
//        public int Index { get; set; }

//        [JsonPropertyName("message")]
//        public OpenAiMessageDto Message { get; set; }

//        [JsonPropertyName("finish_reason")]
//        public string FinishReason { get; set; }
//    }

//    public class OpenAiMessageDto
//    {
//        [JsonPropertyName("role")]
//        public string Role { get; set; }

//        [JsonPropertyName("content")]
//        public string Content { get; set; }
//    }

//    public class OpenAiUsageDto
//    {
//        [JsonPropertyName("prompt_tokens")]
//        public int PromptTokens { get; set; }

//        [JsonPropertyName("completion_tokens")]
//        public int CompletionTokens { get; set; }

//        [JsonPropertyName("total_tokens")]
//        public int TotalTokens { get; set; }
//    }
//}