using OpenAI;
using OpenAI.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ChatConsole;

internal class Program
{
    private static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

    private static Settings _settings = null!;
    private static async Task MainAsync(string[] args)
    {
        var workingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chat-console");
        if (!Directory.Exists(workingDir))
        {
            Directory.CreateDirectory(workingDir);
        }
        var settingsPath = Path.Combine(workingDir, "settings.json");
        _settings = await Settings.FromFile(settingsPath);
        var client = new HttpApiClient(_settings.ApiKey);

        List<ChatMessage> history = new();
        List<ChatMessage> messages = new();
        while (true)
        {
            Console.Write("prompt > ");
            var prompt = Console2.ReadInput();

            if (prompt.StartsWith("/"))
            {
                if (prompt == "/help")
                {
                    ShowHelp();

                }
                else if (prompt == "/quit" || prompt == "/exit")
                {
                    Environment.Exit(0);
                }
                else if (prompt.StartsWith("/reset"))
                {
                    Console.WriteLine("SYSTEM: Conversational context cleared.");
                    messages.Clear();
                    history.Clear();

                }
                else if (prompt == "/clear")
                {
                    Console.Clear();
                    history.Clear();

                }
                else if (prompt == "/export")
                {
                    var exportLogDir = Path.Combine(workingDir, "Logs");
                    if (!Directory.Exists(exportLogDir))
                    {
                        Directory.CreateDirectory(exportLogDir);
                    }
                    var exportFile = Path.Combine(exportLogDir, "Log_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
                    using (StreamWriter sw = new(exportFile, false))
                    {
                        foreach (var msg in history)
                        {
                            sw.WriteLine(msg.Role + ": " + msg.Content);
                        }
                    }
                    Console.WriteLine("SYSTEM: Wrote conversation to " + exportFile);
                }
                else if (prompt == "/prompts")
                {
                    var promptDir = Path.Combine(workingDir, "Prompts");
                    if (!Directory.Exists(promptDir))
                    {
                        Directory.CreateDirectory(promptDir);
                    }
                    Console.WriteLine("SYSTEM: Select a prompt calling `/prompt <promptname>`.");
                    Console.WriteLine("SYSTEM: Unset the prompt using `/prompt` with no argument.");
                    Console.WriteLine("SYSTEM: Available prompts:");
                    var files = Directory.GetFiles(promptDir);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        Console.WriteLine("SYSTEM: \t" + fileName);
                    }
                }
                else if (prompt.StartsWith("/prompt"))
                {
                    if (prompt.Trim() == "/prompt")
                    {
                        _settings.SystemMessage = "";
                        Console.WriteLine("SYSTEM: Unset the prompt.");

                    }
                    else if (prompt.Split(" ").Length > 1)
                    {
                        var wantedPrompt = prompt.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];

                        var promptDir = Path.Combine(workingDir, "Prompts");
                        if (!Directory.Exists(promptDir))
                        {
                            Directory.CreateDirectory(promptDir);
                        }
                        var files = Directory.GetFiles(promptDir);
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileName(file);
                            if (fileName == wantedPrompt)
                            {
                                using (StreamReader promptReader = new(file))
                                {
                                    _settings.SystemMessage = await promptReader.ReadToEndAsync(); ;
                                }
                                Console.WriteLine("SYSTEM: Set the prompt to " + fileName);
                            }
                        }

                    }
                }
                continue;
            }

            var userMessage = new ChatMessage { Role = "user", Content = prompt };
            messages.Add(userMessage);
            history.Add(userMessage);

            var request = new ChatRequest();
            request.Stream = true;

            request.Messages.Clear();
            if (!string.IsNullOrEmpty(_settings.SystemMessage))
            {
                var role = _settings.RequestParams.Model.Contains("gpt-4") ? "system" : "user";
                var systemMessage = new ChatMessage { Role = role, Content = _settings.SystemMessage };
                request.Messages.Add(systemMessage);
            }
            request.Messages.AddRange(messages);

            while (request.CountMessagesTokens() > 4096)
            {
                int removeIndex = !string.IsNullOrEmpty(_settings.SystemMessage) ? 1 : 0;
                messages.RemoveAt(removeIndex);
                if (prompt.Length == removeIndex)
                {
                    Console.Error.WriteLine("SYSTEM: Message too big. Run /reset and retry.");
                    break;
                }

                request.Messages.Clear();
                request.Messages.AddRange(messages);
            }

            var webRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            webRequest.Content = JsonContent.Create<ChatRequest>(request, options: HttpApiClient.DefaultJsonOptions);
            var response = await client.SendAsync(webRequest);

            StringBuilder respMsg = new();
            using StreamReader responseReader = new(await response.Content.ReadAsStreamAsync());
            string? line;
            while ((line = await responseReader.ReadLineAsync()) != null)
            {
                if (!line.StartsWith("data:")) continue;

                line = line[5..];
                if (line.Trim() == "[DONE]") break;

                try
                {
                    var scr = JsonSerializer.Deserialize<StreamedChatResponse>(line, HttpApiClient.DefaultJsonOptions)!;
                    Console.Write(scr.Choices[0].Delta.Content);
                    respMsg.Append(scr.Choices[0].Delta.Content);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }
            Console.WriteLine();
            var assistantMessage = new ChatMessage { Role = "assistant", Content = respMsg.ToString() };
            messages.Add(assistantMessage);
            history.Add(assistantMessage);

        }
    }

    internal static void ShowHelp()
    {
        Console.WriteLine(
"""
                Welcome to the chat-console!! :)

                Only a few commands are built in and all start with a forward-slash (/).
                All other messages are sent directly to OpenAI for a response.

                The built-in commands are:
                Welcome to our chatbot! Here are the available commands to use:

                - /help
                    displays helpful information about the chatbot.

                - /reset
                    clears any previous chat context.

                - /export
                    exports the current chat history to a log.

                - /clear
                    clears the chat window.     

                - /quit or /exit
                    ends the conversation with the chatbot.
                    
""");
    }
}