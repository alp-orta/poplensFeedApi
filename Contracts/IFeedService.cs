using poplensFeedApi.Models;
using poplensFeedApi.Models.Common;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Contracts {
    public interface IFeedService {
        Task<PageResult<ReviewProfileDetail>> GetFollowerFeedAsync(string profileId, string token, int page = 1, int pageSize = 10);
        Task<PageResult<ReviewProfileDetail>> GetForYouFeedAsync(string profileId, string token, int pageSize = 10);  // For You feed logic can be added later
    }

}
