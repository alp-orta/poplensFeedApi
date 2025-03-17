using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using poplensFeedApi.Models.Common;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Contracts {
    public interface IFeedService {
        Task<PageResult<ReviewDetail>> GetFollowerFeedAsync(string profileId, int page = 1, int pageSize = 10);
        Task<PageResult<ReviewDetail>> GetForYouFeedAsync(string profileId);  // For You feed logic can be added later
    }

}
