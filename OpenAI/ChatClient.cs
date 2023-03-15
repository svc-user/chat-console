using chat_console.OpenAI;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace OpenAI;

class ChatClient
{
    private readonly ApiClient _apiClient;

    private ChatRequest _requestParams = new();
    private string _systemMessage = string.Empty;
    private int _historyContextLength = 5;
    private List<ChatMessage> _historyContext = new();

    public delegate void MessageReceived(ChatResponse message);
    public delegate void MessageError(ApiError? error);
    public MessageReceived OnMessageReceived { get; set; } = null!;
    public MessageError OnMessageError { get; set; } = null!;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name="modelId"></param>
    public ChatClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public void SetParams(ChatRequest requestParams, int historyContextLength = 5, string systemMessage = "")
    {
        requestParams.Messages.Clear();
        _requestParams = requestParams;
        _systemMessage = systemMessage;
        _historyContextLength = historyContextLength;
    }

    public void ClearContext() => _historyContext.Clear();

    public async Task SendMessage(string message)
    {
        var userMessage = new ChatMessage { Role = "user", Content = message };

        var chatRequest = _requestParams.Clone();
        if (!string.IsNullOrWhiteSpace(_systemMessage))
        {
            chatRequest.Messages.Add(new ChatMessage { Role = "system", Content = _systemMessage });
        }
        if (_historyContext.Count > _historyContextLength)
        {
            _historyContext = _historyContext.TakeLast(_historyContextLength).ToList();
        }
        chatRequest.Messages.AddRange(_historyContext);
        chatRequest.Messages.Add(userMessage);

        var tokens = chatRequest.CountMessagesTokens();
        Trace.WriteLine($"Using {tokens} token on {chatRequest.Messages.Count} messages for request.");

        var resp = await _apiClient.Post<ChatRequest, ChatResponse>("chat/completions", chatRequest);

        if (!resp.Success)
        {
            OnMessageError?.Invoke(resp?.Error);
            return;
        }
        _historyContext.Add(userMessage);
        _historyContext.AddRange(resp.Response.Choices.Select(c => c.Message));
        OnMessageReceived?.Invoke(resp.Response);
    }
}

public class ChatRequest
{
    public ChatRequest Clone()
    {
        var req = (ChatRequest)this.MemberwiseClone();
        req.Messages.Clear();
        return req;
    }

    public int CountMessagesTokens()
    {
        var allMessages = string.Join(" ", Messages.Select(m => m.Content));
        var tokens = TokenHelper.Tokenize(allMessages);

        var reconstructedMessage = string.Join("", tokens.Select(t => t.Item2));
        if (allMessages != reconstructedMessage)
        {
            Debugger.Break();
        }

        return tokens.Count;
    }

    /// <summary>
    /// ID of the model to use. Currently, only <b>gpt-3.5-turbo</b> and <b>gpt-3.5-turbo-0301</b> are supported.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "gpt-3.5-turbo-0301";

    /// <summary>
    /// The messages to generate chat completions for, in the <a href="https://platform.openai.com/docs/guides/chat/introduction">chat format</a>.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
    /// <br></br>
    /// We generally recommend altering this or <see cref="TopP" /> but not both.
    /// </summary>
    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    /// <summary>
    /// An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.
    /// <br></br>
    /// We generally recommend altering this or <see cref="Temperature"/> but not both.
    /// </summary>
    [JsonPropertyName("top_p")]
    public decimal? TopP { get; set; }

    /// <summary>
    /// How many chat completion choices to generate for each input message.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available, with the stream terminated by a <b>data: [DONE]</b> message.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Up to 4 sequences where the API will stop generating further tokens.
    /// </summary>
    [JsonPropertyName("stop")]
    public string? Stop { get; set; }

    /// <summary>
    /// The maximum number of tokens allowed for the generated answer. By default, the number of tokens the model can return will be (4096 - prompt tokens).
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public uint? MaxTokens { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.
    /// <br></br>
    /// <a href="https://platform.openai.com/docs/api-reference/parameter-details">See more information about frequency and presence penalties.</a>
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public decimal? PresencePenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
    /// <br></br>
    /// <a href="https://platform.openai.com/docs/api-reference/parameter-details">See more information about frequency and presence penalties.</a>
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public decimal? FrequencyPenalty { get; set; }

    /// <summary>
    /// Modify the likelihood of specified tokens appearing in the completion.
    /// <br></br>
    /// <br></br>
    /// Accepts a json object that maps tokens (specified by their token ID in the tokenizer) to an associated bias value from -100 to 100. Mathematically, the bias is added to the logits generated by the model prior to sampling. The exact effect will vary per model, but values between -1 and 1 should decrease or increase likelihood of selection; values like -100 or 100 should result in a ban or exclusive selection of the relevant token.
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int>? LogitBias { get; set; }

    /// <summary>
    /// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public uint Created { get; set; }
    public List<ChatResponseChoice> Choices { get; set; } = new List<ChatResponseChoice>();
    public ChatResponseUsage Usage { get; set; } = new ChatResponseUsage();

}

public class ChatResponseChoice
{
    public int Index { get; set; }
    public ChatMessage Message { get; set; } = new ChatMessage();
    public FinishReason FinishReason { get; set; }
}

public class ChatResponseUsage
{
    [JsonPropertyName("prompt_tokens")]
    public uint PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public uint CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public uint TotalTokens { get; set; }
}

public enum FinishReason
{
    Stop,
    Length,
    [JsonPropertyName("content_filter")] ContentFiler,
    Null
}