namespace NVOAMASIS.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user"; // "user", "assistant", "system"
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class OpenAIChatRequest
    {
        public string model { get; set; } = "gpt-4o-mini";
        public List<OpenAIMessage> messages { get; set; } = new();
        public double temperature { get; set; } = 0.7;
        public int max_tokens { get; set; } = 2048;
    }

    public class OpenAIMessage
    {
        public string role { get; set; } = "";
        public string content { get; set; } = "";
    }

    public class OpenAIChatResponse
    {
        public string? id { get; set; }
        public List<OpenAIChoice>? choices { get; set; }
        public OpenAIUsage? usage { get; set; }
    }

    public class OpenAIChoice
    {
        public OpenAIMessage? message { get; set; }
        public string? finish_reason { get; set; }
    }

    public class OpenAIUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class ChatGPTSettings
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4o-mini";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public int MaxTokens { get; set; } = 2048;
        public double Temperature { get; set; } = 0.7;
    }
}
