using System.Text.Json.Serialization;

namespace OpenAI.Models;

public enum FinishReason
{
    Stop,
    Length,
    [JsonPropertyName("content_filter")] ContentFiler,
    Null
}