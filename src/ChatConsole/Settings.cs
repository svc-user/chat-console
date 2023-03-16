using OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatConsole;

internal class Settings
{
    private static JsonSerializerOptions _defaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true,
    };

    public static async Task<Settings> FromFile(string path)
    {
        if (!File.Exists(path))
        {
            var settingsObject = new Settings();
            using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            await JsonSerializer.SerializeAsync<Settings>(fileStream, settingsObject, _defaultJsonOptions);

            settingsObject._path = path;
            return settingsObject;
        }
        else
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var settingsObject = await JsonSerializer.DeserializeAsync<Settings>(fileStream, _defaultJsonOptions);
            if (settingsObject == null) throw new Exception("Unable to read settings file.");

            settingsObject._path = path;
            return settingsObject;
        }
    }

    public async Task SaveAsync()
    {
        using var fileStream = new FileStream(_path, FileMode.Create, FileAccess.Write);
        RequestParams.Messages.Clear();
        await JsonSerializer.SerializeAsync<Settings>(fileStream, this, _defaultJsonOptions);
    }

    [JsonIgnore]
    private string _path = null!;

    [JsonIgnore]
    public int LongestName { get { return Math.Max(UserName.Length, BotName.Length); } }

    [JsonIgnore]
    public string UserNamePadded { get { return UserName.PadRight(LongestName); } }

    [JsonIgnore]
    public string BotNamePadded { get { return BotName.PadRight(LongestName); } }

    public string ConsoleTitle { get; set; } = "Chat window";
    public string SystemMessage { get; set; } = string.Empty;
    public string UserName { get; set; } = "user";
    public string BotName { get; set; } = "gpt-turbo";
    public string ApiKey { get; set; } = string.Empty;
    public string PS1 { get; set; } = ">";
    public ushort ContextLength { get; set; } = 5;


    public ChatRequest RequestParams { get; set; } = new ChatRequest();
}
