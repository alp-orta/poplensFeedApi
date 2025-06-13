using Newtonsoft.Json;
using Pgvector;
using poplensFeedApi.Helpers;
using poplensMediaApi.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace poplensFeedApi.Services {
    public interface IMediaApiProxyService {
        Task<Media> GetMediaByIdAsync(Guid id, string authorizationToken); 
        Task<Media> GetMediaWithEmbeddingById(Guid id, string authorizationToken);
        Task<List<Media>> GetSimilarMediaAsync(Vector embedding, int count, string authorizationToken, string? mediaType = null, List<Guid>? excludedMediaIds = null);
    }
    public class MediaApiProxyService : IMediaApiProxyService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _mediaApiUrl = "http://poplensMediaApi:8080/api/";

        public MediaApiProxyService(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClientWithAuthorization(string token) {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<Media> GetMediaByIdAsync(Guid id, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_mediaApiUrl}Media/{id}");

            if (!response.IsSuccessStatusCode) {
                return null;
            }

            var mediaJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Media>(mediaJson);
        }
        public async Task<Media> GetMediaWithEmbeddingById(Guid id, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_mediaApiUrl}Media/GetMediaWithEmbeddingById/{id}");

            if (!response.IsSuccessStatusCode) {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Media>(_jsonOptions);
        }

        public async Task<List<Media>> GetSimilarMediaAsync(
    Vector embedding,
    int count,
    string authorizationToken,
    string? mediaType = null,
    List<Guid>? excludedMediaIds = null) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var embeddingArray = embedding.ToArray();

            // Debug embedding
            Console.WriteLine($"[Proxy.GetSimilarMediaAsync] Embedding length: {embeddingArray.Length}");
            if (embeddingArray.Length > 0)
                Console.WriteLine($"[Proxy.GetSimilarMediaAsync] Embedding sample: [{string.Join(", ", embeddingArray.Take(5))}...]");

            var requestBody = new {
                Embedding = embeddingArray,
                Count = count,
                MediaType = mediaType,
                ExcludedMediaIds = excludedMediaIds
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_mediaApiUrl}Media/Similar", content);

            if (!response.IsSuccessStatusCode) {
                Console.WriteLine($"[Proxy.GetSimilarMediaAsync] API call failed: {response.StatusCode}");
                return new List<Media>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<Media>>(_jsonOptions);
            Console.WriteLine($"[Proxy.GetSimilarMediaAsync] Received {result?.Count ?? 0} results from API");
            return result;
        }


        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
        };
        static MediaApiProxyService() {
            _jsonOptions.Converters.Add(new VectorJsonConverter());
        }
    }
}
