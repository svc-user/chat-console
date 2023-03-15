using System.Net.Http.Json;
using System.Text.Json;

namespace OpenAI;

public class ApiClient
{
    private readonly JsonSerializerOptions _defaultJsonOptions;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.openai.com/v1/";
    public ApiClient(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        _defaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        };
    }

    public async Task<ApiResponse<TRes>> Get<TRes>(string requestPath)
    {
        try
        {
            var resp = await _httpClient.GetAsync(requestPath);
            return await CreateResponse<TRes>(resp);
        }
        catch (HttpRequestException ex)
        {
            var apiResp = new ApiResponse<TRes>();
            apiResp.Success = false;
            apiResp.Error = new() { Message = ex.Message };
            apiResp.HttpCode = (int?)ex?.StatusCode ?? 0;
            return apiResp;
        }
    }

    public async Task<ApiResponse<TRes>> Post<TReq, TRes>(string requestPath, TReq requestObject)
    {
        try
        {
            var postContent = JsonContent.Create<TReq>(requestObject, options: _defaultJsonOptions);
            var resp = await _httpClient.PostAsync(requestPath, postContent);

            return await CreateResponse<TRes>(resp);
        }
        catch (HttpRequestException ex)
        {
            var apiResp = new ApiResponse<TRes>();
            apiResp.Success = false;
            apiResp.Error = new() { Message = ex.Message };
            apiResp.HttpCode = (int?)ex?.StatusCode ?? 0;
            return apiResp;
        }
    }

    private async Task<ApiResponse<TRes>> CreateResponse<TRes>(HttpResponseMessage responseMessage)
    {
        var apiResp = new ApiResponse<TRes>();

        var respBody = await responseMessage.Content.ReadAsStringAsync();
        if (!responseMessage.IsSuccessStatusCode)
        {
            apiResp.Success = false;
            apiResp.Error = JsonSerializer.Deserialize<ApiResponse<TRes>>(respBody, _defaultJsonOptions)?.Error;
            apiResp.HttpCode = (int)responseMessage.StatusCode;

            return apiResp;
        }

        apiResp.Success = true;
        apiResp.Response = JsonSerializer.Deserialize<TRes>(respBody, _defaultJsonOptions)!;

        return apiResp;

    }
}

public class ApiResponse<TRes> 
{
    public TRes Response { get; set; } = default(TRes)!;
    public bool Success { get; set; }
    public ApiError? Error { get; set; }
    public int HttpCode { get; set; }
}

public class ApiError
{
    public string Message { get; set; } = string.Empty; // "message": "None is not of type 'object' - 'logit_bias'",
    public string Type { get; set; } = string.Empty; //  "type": "invalid_request_error",
    public string? Param { get; set; } //  "param": null,
    public string? Code { get; set; } // "code": null
}