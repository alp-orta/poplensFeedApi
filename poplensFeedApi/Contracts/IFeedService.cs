using poplensFeedApi.Models;
using poplensFeedApi.Models.Common;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Contracts {
    public interface IFeedService {
        Task<PageResult<ReviewProfileDetail>> GetFollowerFeedAsync(string profileId, string token, int page = 1, int pageSize = 10);
        Task<PageResult<ReviewDetail>> GetForYouFeedAsync(string profileId);  // For You feed logic can be added later
    }

}
