using System.Text.Json.Serialization;

namespace OpenAI.Models;

public class ChatResponseUsage
{
    [JsonPropertyName("prompt_tokens")]
    public uint PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public uint CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public uint TotalTokens { get; set; }
}
