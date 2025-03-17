using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poplensFeedApi.Contracts;
using poplensFeedApi.Models.Common;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService) {
            _feedService = feedService;
        }

        [HttpGet("GetFollowerFeed/{profileId}")]
        public async Task<ActionResult<PageResult<ReviewDetail>>> GetFollowerFeed(string profileId, int page = 1, int pageSize = 10) {
            try {
                var reviews = await _feedService.GetFollowerFeedAsync(profileId, page, pageSize);
                return Ok(reviews);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("for-you/{profileId}")]
        public async Task<ActionResult<PageResult<ReviewDetail>>> GetForYouFeed(string profileId) {
            try {
                var reviews = await _feedService.GetForYouFeedAsync(profileId);
                return Ok(reviews);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}
