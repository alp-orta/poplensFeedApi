using poplensFeedApi.Contracts;
using poplensFeedApi.Controllers;
using poplensFeedApi.Models.Common;
using poplensMediaApi.Models;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Services {
    public class FeedService : IFeedService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _userProfileApiUrl = "https://localhost:7056/api/";
        private readonly MediaApiProxyController _mediaApiProxyController;

        public FeedService(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
            _mediaApiProxyController = _mediaApiProxyController;
        }

        public async Task<PageResult<ReviewDetail>> GetFollowerFeedAsync(string profileId, int page = 1, int pageSize = 10) {
            var response = new PageResult<ReviewDetail> {
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                Result = new List<ReviewDetail>()
            };
            var client = _httpClientFactory.CreateClient();
            var followingResponse = await client.GetAsync($"{_userProfileApiUrl}Profile/{profileId}/followinglist");

            if (!followingResponse.IsSuccessStatusCode) {
                throw new Exception("Error fetching followed users.");
            }

            var followedUserIds = await followingResponse.Content.ReadFromJsonAsync<List<Guid>>();
            if (followedUserIds != null && followedUserIds.Count == 0) {
                return response;
            }
            
            var reviews = new List<Review>();
            foreach (var followId in followedUserIds) {
                var reviewResponse = await client.GetAsync($"{_userProfileApiUrl}Review/{followId}/reviews?page={page}&pageSize={pageSize}");
                if (reviewResponse.IsSuccessStatusCode) {
                    var userReviews = await reviewResponse.Content.ReadFromJsonAsync<List<Review>>();
                    reviews.AddRange(userReviews);
                }
            }
            var detailedReviews = await GetReviewDetailsAsync(reviews);
            response.TotalCount = detailedReviews.Count;
            response.Result = detailedReviews.OrderByDescending(r => r.CreatedDate).Take(pageSize).ToList();
            return response;
        }

        private async Task<List<ReviewDetail>> GetReviewDetailsAsync(List<Review> reviews) {
            var reviewDetails = new List<ReviewDetail>();

            foreach (var review in reviews) {
                var media = await _mediaApiProxyController.GetMediaById(Guid.Parse(review.MediaId));
                if (media != null) {
                    var reviewDetail = new ReviewDetail {
                        Id = review.Id,
                        Content = review.Content,
                        Rating = review.Rating,
                        ProfileId = review.ProfileId,
                        MediaId = review.MediaId,
                        CreatedDate = review.CreatedDate,
                        LastUpdatedDate = review.LastUpdatedDate,
                        // Fetch additional fields from another API
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
        private string GetCreator(Media media) {
            return media.Type switch {
                "film" => media.Director,
                "book" => media.Writer,
                "game" => media.Publisher,
                _ => "Unknown"
            };
        }


        public Task<PageResult<ReviewDetail>> GetForYouFeedAsync(string profileId) {
            // For You Feed logic to be added later
            throw new NotImplementedException();
        }
    }

}
