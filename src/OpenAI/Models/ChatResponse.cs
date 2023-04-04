namespace OpenAI.Models;

public class ChatResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public uint Created { get; set; }
    public List<ChatResponseChoice> Choices { get; set; } = new List<ChatResponseChoice>();
    public ChatResponseUsage Usage { get; set; } = new ChatResponseUsage();

}
