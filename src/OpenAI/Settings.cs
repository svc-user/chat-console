using OpenAI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAI;

public class Settings
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

            return settingsObject;
        }
        else
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var settingsObject = await JsonSerializer.DeserializeAsync<Settings>(fileStream, _defaultJsonOptions);
            if (settingsObject == null) throw new Exception("Unable to read settings file.");

            return settingsObject;
        }
    }


    public string SystemMessage { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;


    public ChatRequest RequestParams { get; set; } = new ChatRequest();
}
