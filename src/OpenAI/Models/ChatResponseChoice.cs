namespace OpenAI.Models;

public class ChatResponseChoice
{
    public int Index { get; set; }
    public ChatMessage Message { get; set; } = new ChatMessage();
    public FinishReason FinishReason { get; set; }
}
