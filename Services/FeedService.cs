using poplensFeedApi.Contracts;
using poplensFeedApi.Models;
using poplensFeedApi.Models.Common;
using poplensMediaApi.Models;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Services {
    public class FeedService : IFeedService {
        private readonly IUserProfileApiProxyService _userProfileApiProxyService;
        private readonly IMediaApiProxyService _mediaApiProxyService;

        public FeedService(IUserProfileApiProxyService userProfileApiProxyService, IMediaApiProxyService mediaApiProxyService) {
            _userProfileApiProxyService = userProfileApiProxyService;
            _mediaApiProxyService = mediaApiProxyService;
        }

        public async Task<PageResult<ReviewProfileDetail>> GetFollowerFeedAsync(string profileId, string token, int page = 1, int pageSize = 10) {
            var response = new PageResult<ReviewProfileDetail> {
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                Result = new List<ReviewProfileDetail>()
            };

            // Fetch followed profiles using the token for authorization
            var followedProfiles = await _userProfileApiProxyService.GetFollowingListAsync(profileId, token);
            if (followedProfiles == null || followedProfiles.Count == 0) {
                return response;
            }

            // Extract profile IDs from followed profiles
            var profileIds = followedProfiles.Select(p => p.ProfileId).ToList();

            // Fetch reviews from followed profiles using the new method
            var reviews = await _userProfileApiProxyService.GetReviewsByProfileIdsAsync(profileIds, page, pageSize, token);

            // Fetch detailed reviews with media info
            var detailedReviews = await GetReviewDetailsAsync(reviews, token);

            // Map to detailed response format including profile info
            var detailedProfileReviews = MapToReviewProfileDetails(detailedReviews, followedProfiles);

            response.TotalCount = detailedProfileReviews.Count;
            response.Result = detailedProfileReviews.OrderByDescending(r => r.CreatedDate).Take(pageSize).ToList();
            return response;
        }

        private async Task<List<ReviewDetail>> GetReviewDetailsAsync(List<Review> reviews, string token) {
            var reviewDetails = new List<ReviewDetail>();

            foreach (var review in reviews) {
                var media = await _mediaApiProxyService.GetMediaByIdAsync(Guid.Parse(review.MediaId), token);
                if (media != null) {
                    var reviewDetail = new ReviewDetail {
                        Id = review.Id,
                        Content = review.Content,
                        Rating = review.Rating,
                        ProfileId = review.ProfileId,
                        MediaId = review.MediaId,
                        CreatedDate = review.CreatedDate,
                        LastUpdatedDate = review.LastUpdatedDate,
                        MediaTitle = media.Title,
                        MediaType = media.Type,
                        MediaCachedImagePath = media.CachedImagePath,
                        MediaCreator = GetCreator(media)
                    };

                    reviewDetails.Add(reviewDetail);
                }
            }
            return reviewDetails.OrderByDescending(r => r.CreatedDate).ToList();
        }

        private List<ReviewProfileDetail> MapToReviewProfileDetails(List<ReviewDetail> detailedReviews, List<FollowedProfile> followedProfiles) {
            return detailedReviews.Select(detailedReview => new ReviewProfileDetail {
                Id = detailedReview.Id,
                Content = detailedReview.Content,
                Rating = detailedReview.Rating,
                ProfileId = detailedReview.ProfileId,
                MediaId = detailedReview.MediaId,
                CreatedDate = detailedReview.CreatedDate,
                LastUpdatedDate = detailedReview.LastUpdatedDate,
                MediaTitle = detailedReview.MediaTitle,
                MediaType = detailedReview.MediaType,
                MediaCachedImagePath = detailedReview.MediaCachedImagePath,
                MediaCreator = detailedReview.MediaCreator,
                Username = followedProfiles.FirstOrDefault(p => p.ProfileId == detailedReview.ProfileId)?.Username ?? "Unknown"
            }).ToList();
        }

        private string GetCreator(Media media) {
            return media.Type switch {
                "film" => media.Director,
                "book" => media.Writer,
                "game" => media.Publisher,
                _ => "Unknown"
            };
        }

        public Task<PageResult<ReviewDetail>> GetForYouFeedAsync(string profileId) {
            throw new NotImplementedException();
        }
    }
}
