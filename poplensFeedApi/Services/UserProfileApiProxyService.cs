using poplensUserProfileApi.Models;
using System.Net.Http.Headers;

namespace poplensFeedApi.Services {
    public interface IUserProfileApiProxyService {
        Task<List<FollowedProfile>> GetFollowingListAsync(string profileId, string authorizationToken);
        Task<List<Review>> GetReviewsAsync(string profileId, int page, int pageSize, string authorizationToken);
    }

    public class UserProfileApiProxyService : IUserProfileApiProxyService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _userProfileApiUrl = "https://localhost:7056/api/";

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

        public async Task<List<Review>> GetReviewsAsync(string profileId, int page, int pageSize, string authorizationToken) {
            var client = CreateHttpClientWithAuthorization(authorizationToken);
            var response = await client.GetAsync($"{_userProfileApiUrl}Review/{profileId}/reviews?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Review>>();
        }
    }

}
