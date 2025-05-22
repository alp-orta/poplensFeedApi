using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using poplensFeedApi.Contracts;
using poplensFeedApi.Models;
using poplensFeedApi.Models.Common;

namespace poplensFeedApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService) {
            _feedService = feedService;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("GetFollowerFeed/{profileId}")]
        public async Task<ActionResult<PageResult<ReviewProfileDetail>>> GetFollowerFeed(string profileId, int page = 1, int pageSize = 10) {
            // Extract the token from the incoming request's Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader)) {
                return Unauthorized(new { message = "Authorization token is missing." });
            }
            // Remove "Bearer " prefix if present
            var token = authHeader.Replace("Bearer ", "");

            try {
                // Pass the token to the service along with the other parameters
                var reviews = await _feedService.GetFollowerFeedAsync(profileId, token, page, pageSize);
                return Ok(reviews);
            } catch (System.Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("GetForYouFeed/{profileId}")]
        public async Task<ActionResult<PageResult<ReviewProfileDetail>>> GetForYouFeed(string profileId, int pageSize = 10) {
            // Extract the token from the incoming request's Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader)) {
                return Unauthorized(new { message = "Authorization token is missing." });
            }
            // Remove "Bearer " prefix if present
            var token = authHeader.Replace("Bearer ", "");

            try {
                // Pass the token to the service along with the other parameters
                var recommendations = await _feedService.GetForYouFeedAsync(profileId, token, pageSize);
                return Ok(recommendations);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
