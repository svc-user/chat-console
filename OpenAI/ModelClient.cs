using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAI;

public class ModelClient
{
    private readonly ApiClient _apiClient;
    public ModelClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ModelResponse?> ListModels()
    {
        var resp = await _apiClient.Get<ModelResponse>("models");
        return resp.Success ? resp.Response : null;

    }

    public async Task<ModelData?> GetById(string model)
    {
        var resp = await _apiClient.Get<ModelData>($"models/{model}");
        return resp.Success ? resp.Response : null;

    }
}

public class ModelResponse
{
    public List<ModelData> Data { get; set; } = new List<ModelData>();
}

public class ModelData
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public uint Created { get; set; }
    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;
    public List<ModelPermissions> Permission { get; set; } = new List<ModelPermissions>();

}

public class ModelPermissions
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = string.Empty;

    public uint Created { get; set; }

    [JsonPropertyName("allow_create_engine")]
    public bool AllowCreateEngine { get; set; }

    [JsonPropertyName("allow_sampling")]
    public bool AllowSampling { get; set; }

    [JsonPropertyName("allow_logprobs")]
    public bool AllowLogprobs { get; set; }

    [JsonPropertyName("allow_search_indicies")]
    public bool AllowSearchIndices { get; set; }

    [JsonPropertyName("allow_view")]
    public bool AllowView { get; set; }

    [JsonPropertyName("allow_fine_tuning")]
    public bool AllowFineTuning { get; set; }

    public string Organization { get; set; } = string.Empty;

    public object Group { get; set; } = null!;

    [JsonPropertyName("is_blocking")]
    public bool IsBlocking { get; set; }

}