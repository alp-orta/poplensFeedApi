using poplensFeedApi.Models.Feed;
using Pgvector;
using poplensFeedApi.Helpers;
using poplensFeedApi.Models;
using poplensUserProfileApi.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace poplensFeedApi.Services {
    public interface IUserProfileApiProxyService {
        Task<List<FollowedProfile>> GetFollowingListAsync(string profileId, string authorizationToken);
        Task<List<FollowedProfile>> GetProfilesWithUsernamesAsync(List<Guid> profileIds, string authorizationToken);
        Task<List<Review>> GetReviewsAsync(string profileId, int page, int pageSize, string authorizationToken);
        Task<List<Review>> GetReviewsByProfileIdsAsync(List<Guid> profileIds, int page, int pageSize, string authorizationToken);
        Task<List<Review>> GetReviewsWithEmbeddingsAsync(string profileId, string authorizationToken);
        Task<List<Review>> GetLikedReviewsWithEmbeddingsAsync(string profileId, string authorizationToken);
        Task<List<Review>> GetCommentedReviewsWithEmbeddingsAsync(string profileId, string authorizationToken);
        Task<List<Review>> GetSimilarReviewsAsync(Vector embedding, int count, string authorizationToken, List<Guid>? excludedReviewIds = null, Guid? requestingProfileId = null);
        Task<UserInteractionsResponse> GetUserInteractionsWithEmbeddingsAsync(string profileId, string authorizationToken);

    }

    public class UserProfileApiProxyService : IUserProfileApiProxyService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _userProfileApiUrl = "http://poplensUserProfileApi:8080/api/";

        public UserProfileApiProxyService(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClientWithAuthorization(string token) {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<List<FollowedProfile>> GetFollowingListAsync(string profileId, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Profile/{profileId}/followinglist");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FollowedProfile>>();
        }

        public async Task<List<FollowedProfile>> GetProfilesWithUsernamesAsync(List<Guid> profileIds, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var queryString = string.Join("&", profileIds.Select(id => $"profileIds={id}"));
            var response = await client.GetAsync($"{_userProfileApiUrl}Profile/GetProfilesWithUsernames?{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FollowedProfile>>();
        }

        public async Task<List<Review>> GetReviewsAsync(string profileId, int page, int pageSize, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/reviews?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>();
        }

        public async Task<List<Review>> GetReviewsByProfileIdsAsync(List<Guid> profileIds, int page, int pageSize, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var query = $"?page={page}&pageSize={pageSize}&" + string.Join("&", profileIds.Select(id => $"profileIds={id}"));
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/GetReviewsByProfileIds{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>();
        }

        public async Task<List<Review>> GetReviewsWithEmbeddingsAsync(string profileId, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/GetReviewsWithEmbeddings");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>(_jsonOptions);
        }

        public async Task<List<Review>> GetLikedReviewsWithEmbeddingsAsync(string profileId, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/GetLikedReviewsWithEmbeddings");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>(_jsonOptions);
        }

        public async Task<List<Review>> GetCommentedReviewsWithEmbeddingsAsync(string profileId, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/GetCommentedReviewsWithEmbeddings");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>(_jsonOptions);
        }

        public async Task<List<Review>> GetSimilarReviewsAsync(
            Vector embedding,
            int count,
            string authorizationToken,
            List<Guid>? excludedReviewIds = null,
            Guid? requestingProfileId = null) 
        {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var embeddingArray = embedding.ToArray();

            var content = JsonContent.Create(new SimilarReviewRequest {
                Embedding = embeddingArray,
                Count = count,
                ExcludedReviewIds = excludedReviewIds ?? new List<Guid>(),
                RequestingProfileId = requestingProfileId
            });

            var response = await client.PostAsync($"{_userProfileApiUrl}Review/GetSimilarReviews", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>(_jsonOptions);
        }



        public async Task<UserInteractionsResponse> GetUserInteractionsWithEmbeddingsAsync(string profileId, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/GetUserInteractions");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserInteractionsResponse>(_jsonOptions);
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
        };
        static UserProfileApiProxyService() {
            _jsonOptions.Converters.Add(new VectorJsonConverter());
        }

    }

}
