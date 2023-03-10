
using chat_console;
using OpenAI;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

internal class Program
{
    private static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

    private static Settings _settings = null!;
    private static async Task MainAsync(string[] args)
    {
        var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "chat-console");
        if (!Directory.Exists(settingsPath))
        {
            Directory.CreateDirectory(settingsPath);
        }
        settingsPath = Path.Combine(settingsPath, "settings.json");
        _settings = await Settings.FromFile(settingsPath);
        //const string apiKey = "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";

        var apiClient = new ApiClient(_settings.ApiKey);
        var chatClient = new ChatClient(apiClient);

        Console.Title = _settings.ConsoleTitle;
        await EnterMainLoop(chatClient);
    }

    private static async Task EnterMainLoop(ChatClient chatClient)
    {
        var historyContext = new List<ChatMessage>();
        while (true)
        {
            Console.Write(_settings.UserNamePadded + " " + _settings.PS1 + " ");
            var prompt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                continue;
            }

            if (prompt == "/help")
            {
                SettingsHelper.ShowHelp();

                continue;
            }
            else if (prompt == "/quit" || prompt == "/exit")
            {
                Environment.Exit(0);
            }
            else if (prompt.StartsWith("/get "))
            {
                SettingsHelper.GetSettings(_settings, prompt[5..]);
                continue;
            }
            else if (prompt.StartsWith("/set "))
            {
                var key = prompt[5..].Split(" ")[0];
                var keyIndex = prompt.IndexOf(key);
                await SettingsHelper.SetSetting(_settings, key, prompt[(keyIndex + key.Length + 1)..]);

                Console.Title = _settings.ConsoleTitle;
                continue;
            }
            else if (prompt.StartsWith("/reset "))
            {
                var key = prompt[7..].Split(" ")[0];
                await SettingsHelper.ResetSetting(_settings, key);

                Console.Title = _settings.ConsoleTitle;
                continue;
            }
            else if (prompt == "/clear")
            {
                Console.Clear();
                continue;
            }

            if(string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                Console.WriteLine("====================================================");
                Console.WriteLine("No APIKey found. Set one first.");
                Console.WriteLine("After setting one please restart the application.");
                Console.WriteLine("====================================================");
                Console.WriteLine();
                SettingsHelper.ShowHelp();
                continue;
            }


            var message = new ChatMessage();
            message.Role = "user";
            message.Content = prompt;

            var req = _settings.RequestParams.Clone();
            if (!string.IsNullOrWhiteSpace(_settings.SystemMessage))
            {
                req.Messages.Add(new ChatMessage { Role = "system", Content = _settings.SystemMessage });
            }
            req.Messages.AddRange(historyContext);
            req.Messages.Add(message);

            Console.Write("Awaiting response...");
            var resp = await chatClient.Chat(req);
            Console.Write("\r".PadRight(21 + _settings.LongestName) + "\r"); // Clear line and return curser to start position.

            if (resp == null)
            {
                Console.WriteLine("ERR: Chat request failed. Please try again :)");

                continue;
            }

            Console.WriteLine(_settings.BotNamePadded + " " + _settings.PS1 + " " + resp.Choices[0].Message.Content);
            Console.WriteLine();

            historyContext.Add(resp.Choices[0].Message);

            while (historyContext.Count > _settings.ContextLength)
            {
                historyContext.RemoveAt(0);
            }

        }
    }
}