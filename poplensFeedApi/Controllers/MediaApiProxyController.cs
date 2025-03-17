using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using poplensMediaApi.Models;

namespace poplensFeedApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class MediaApiProxyController : ControllerBase {
        private readonly HttpClient _httpClient;

        public MediaApiProxyController(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        // Get a single media item by ID
        [HttpGet("{id}")]
        public async Task<Media> GetMediaById(Guid id) {
            var response = await _httpClient.GetAsync($"https://localhost:7207/api/Media/{id}");
            if (!response.IsSuccessStatusCode) {
                return null;
            }

            var mediaJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Media>(mediaJson);
        }
    }
}
