
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
        Console.Title = _settings.ConsoleTitle;

        Console.CancelKeyPress += Console_CancelKeyPress;


        var apiClient = new ApiClient(_settings.ApiKey);
        var chatClient = new ChatClient(apiClient);
        chatClient.SetParams(_settings.RequestParams, _settings.ContextLength, _settings.SystemMessage);
        chatClient.OnMessageError += async err =>
        {
            await Console.Error.WriteLineAsync();
            await Console.Error.WriteLineAsync();
            await Console.Error.WriteLineAsync("API ERR: Mesg: " + err?.Message);
            await Console.Error.WriteLineAsync("API ERR: Type: " + err?.Type);
            await Console.Error.WriteLineAsync("API ERR: Code: " + err?.Code);
            await Console.Error.WriteLineAsync();
        };
        chatClient.OnMessageReceived += HandleMessageReceived;
        await MainLoop(chatClient);
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Console.Write("\r".PadRight(80 + _settings.LongestName) + "\r"); // Clear line and return curser to start position.
        Console.Write(_settings.UserNamePadded + " " + _settings.PS1 + " ");
    }

    private static async Task MainLoop(ChatClient chatClient)
    {
        while (true)
        {
            Console.Write(_settings.UserNamePadded + " " + _settings.PS1 + " ");
            var prompt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                continue;
            }

            if (await HandleBuiltinCommand(prompt, chatClient))
            {
                continue;
            }


            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                Console.WriteLine("====================================================");
                Console.WriteLine("No APIKey found. Set one first.");
                Console.WriteLine("After setting one please restart the application.");
                Console.WriteLine("====================================================");
                Console.WriteLine();
                SettingsHelper.ShowHelp();
                continue;
            }



            Console.Write("Awaiting response...");
            await chatClient.SendMessage(prompt);
        }
    }

    private static async Task<bool> HandleBuiltinCommand(string prompt, ChatClient chatClient)
    {
        if (prompt == "/help")
        {
            SettingsHelper.ShowHelp();

            return true;
        }
        else if (prompt == "/quit" || prompt == "/exit")
        {
            Environment.Exit(0);
        }
        else if (prompt.StartsWith("/get "))
        {
            SettingsHelper.GetSettings(_settings, prompt[5..]);
            return true;
        }
        else if (prompt.StartsWith("/set "))
        {
            var key = prompt[5..].Split(" ")[0];
            var keyIndex = prompt.IndexOf(key);
            await SettingsHelper.SetSetting(_settings, key, prompt[(keyIndex + key.Length + 1)..]);

            Console.Title = _settings.ConsoleTitle;
            chatClient.SetParams(_settings.RequestParams, _settings.ContextLength, _settings.SystemMessage);

            return true;
        }
        else if (prompt.StartsWith("/reset "))
        {
            var key = prompt[7..].Split(" ")[0];
            await SettingsHelper.ResetSetting(_settings, key);

            Console.Title = _settings.ConsoleTitle;
            chatClient.SetParams(_settings.RequestParams, _settings.ContextLength, _settings.SystemMessage);

            return true;
        }
        else if (prompt == "/clear")
        {
            Console.Clear();
            return true;
        }
        else if (prompt == "/clearcontext")
        {
            chatClient.ClearContext();
            return true;
        }

        return false;
    }

    private static void HandleMessageReceived(ChatResponse chatResponse)
    {
        Console.Write("\r".PadRight(21 + _settings.LongestName) + "\r"); // Clear line and return curser to start position.

        foreach (var choice in chatResponse.Choices)
        {
            if (chatResponse.Choices.Count > 1)
            {
                Console.WriteLine("".PadRight(_settings.LongestName, '-'));
                Console.WriteLine($"Reply {choice.Index + 1} of {chatResponse.Choices.Count}");
            }

            Console.Write(_settings.BotNamePadded + " " + _settings.PS1 + " ");
            var padLength = _settings.LongestName + 3;
            var messageLines = choice.Message.Content.Split("\n");
            bool first = true;
            foreach (var line in messageLines)
            {
                if (!first)
                {
                    Console.Write("".PadRight(padLength));
                }
                first = false;
                PrintLine(padLength, line);
            }
        }
    }

    private static void PrintLine(int padding, string message)
    {
        int col = 0;
        for (int i = 0; i < message.Length; i++, col++)
        {
            var ch = message[i];
            Console.Write(ch);
            if (col > 80 && ch == ' ')
            {
                Console.Write("\n".PadRight(padding + 1));
                col = 0;
            }
            Thread.Sleep(4);
        }
        Console.WriteLine();
    }
}