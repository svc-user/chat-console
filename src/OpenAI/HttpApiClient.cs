using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAI
{
    public class HttpApiClient : HttpClient
    {
        public static readonly JsonSerializerOptions DefaultJsonOptions;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.openai.com/v1/";

        static HttpApiClient()
        {
            DefaultJsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            };
        }

        public HttpApiClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }

        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}
