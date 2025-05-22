using Microsoft.EntityFrameworkCore;
using Pgvector;
using poplensFeedApi.Contracts;
using poplensFeedApi.Data;
using poplensFeedApi.Models;
using poplensFeedApi.Models.Common;
using poplensMediaApi.Models;
using poplensUserProfileApi.Models;

namespace poplensFeedApi.Services {
    public class FeedService : IFeedService {
        private readonly IUserProfileApiProxyService _userProfileApiProxyService;
        private readonly IMediaApiProxyService _mediaApiProxyService;
        private readonly FeedDbContext _dbContext;

        public FeedService(IUserProfileApiProxyService userProfileApiProxyService, IMediaApiProxyService mediaApiProxyService, FeedDbContext dbContext) {
            _userProfileApiProxyService = userProfileApiProxyService;
            _mediaApiProxyService = mediaApiProxyService;
            _dbContext = dbContext;
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

        public async Task<PageResult<ReviewProfileDetail>> GetForYouFeedAsync(string profileId, string token, int pageSize = 10) {
            var profileGuid = Guid.Parse(profileId);
            var response = new PageResult<ReviewProfileDetail> {
                Page = 1, // For You feed is not paginated
                PageSize = pageSize,
                TotalCount = 0,
                Result = new List<ReviewProfileDetail>()
            };

            try {
                // Define weight factors
                const float OwnReviewWeight = 1.0f;
                const float CommentedReviewWeight = 0.7f;
                const float LikedReviewWeight = 0.3f;

                // Define time decay parameters (from 1.0 to 0.7 over time)
                const float MinTimeFactor = 0.7f;
                const float MaxTimeFactor = 1.0f;
                // Reviews older than this will get minimum weight factor
                TimeSpan maxAgeForFullWeight = TimeSpan.FromDays(30); // 1 month

                // Collection to store weighted embeddings with their weights
                List<(Vector Embedding, float Weight)> weightedEmbeddings = new();

                // Get all user interactions in a single API call
                var userInteractions = await _userProfileApiProxyService.GetUserInteractionsWithEmbeddingsAsync(profileId, token);

                // 1. Process user's own reviews (weight = 1.0)
                foreach (var review in userInteractions.OwnReviews.Where(r => r.Embedding != null)) {
                    float timeFactor = CalculateTimeFactor(review.CreatedDate, MinTimeFactor, MaxTimeFactor, maxAgeForFullWeight);
                    weightedEmbeddings.Add((review.Embedding, OwnReviewWeight * timeFactor));
                }

                // 2. Process reviews the user has commented on (weight = 0.7)
                foreach (var review in userInteractions.CommentedReviews.Where(r => r.Embedding != null)) {
                    float timeFactor = CalculateTimeFactor(review.CreatedDate, MinTimeFactor, MaxTimeFactor, maxAgeForFullWeight);
                    weightedEmbeddings.Add((review.Embedding, CommentedReviewWeight * timeFactor));
                }

                // 3. Process reviews the user has liked (weight = 0.3)
                foreach (var review in userInteractions.LikedReviews.Where(r => r.Embedding != null)) {
                    float timeFactor = CalculateTimeFactor(review.CreatedDate, MinTimeFactor, MaxTimeFactor, maxAgeForFullWeight);
                    weightedEmbeddings.Add((review.Embedding, LikedReviewWeight * timeFactor));
                }

                // If we don't have any taste profile data, return empty result
                if (weightedEmbeddings.Count == 0) {
                    return response;
                }

                // 4. Aggregate the embeddings with their weights to create a taste profile
                Vector aggregatedEmbedding = AggregateWeightedEmbeddings(weightedEmbeddings);

                // Get reviews that have already been displayed to the user
                var displayedReviewIds = await GetDisplayedReviewsForUserAsync(profileGuid);

                // 5. Get reviews with embeddings similar to the taste profile, excluding already displayed reviews
                var similarReviews = await _userProfileApiProxyService.GetSimilarReviewsAsync(
                    aggregatedEmbedding, pageSize, token, displayedReviewIds);

                // 6. Record these reviews as displayed to the user
                await AddDisplayedReviewsAsync(profileGuid, similarReviews.Select(r => r.Id).ToList());

                // 7. Get detailed review information including media details
                var detailedReviews = await GetReviewDetailsAsync(similarReviews, token);

                // 8. Fetch usernames for the reviews' profile IDs
                var profileIds = detailedReviews.Select(r => r.ProfileId).Distinct().ToList();
                var profilesWithUsernames = await _userProfileApiProxyService.GetProfilesWithUsernamesAsync(profileIds, token);

                // 9. Map to detailed response format including profile info using the existing mapping method
                var profileDetailedReviews = MapToReviewProfileDetails(detailedReviews, profilesWithUsernames);

                response.Result = profileDetailedReviews;
                response.TotalCount = profileDetailedReviews.Count;
                return response;
            } catch (Exception ex) {
                // Log the exception
                Console.WriteLine($"Error in GetForYouFeedAsync: {ex}");
                return response;
            }
        }

        // Helper method to calculate time decay factor
        private float CalculateTimeFactor(DateTime createdDate, float minFactor, float maxFactor, TimeSpan maxAge) {
            var age = DateTime.UtcNow - createdDate;
            if (age <= TimeSpan.Zero) return maxFactor;
            if (age >= maxAge) return minFactor;
             
            var decayFactor = Math.Log(1 + age.TotalDays) / Math.Log(1 + maxAge.TotalDays);
            return maxFactor - (float)(decayFactor * (maxFactor - minFactor));
        }

        // Method to aggregate weighted embeddings
        private Vector AggregateWeightedEmbeddings(List<(Vector Embedding, float Weight)> weightedEmbeddings) {
            if (weightedEmbeddings == null || weightedEmbeddings.Count == 0)
                return null;

            int dimension = 384; // Based on the vector dimension in the Review model
            float[] sumVector = new float[dimension];
            float totalWeight = 0f;

            // Sum all embeddings with their weights
            foreach (var (embedding, weight) in weightedEmbeddings) {
                if (embedding == null) continue;

                var embeddingArray = embedding.ToArray();
                for (int i = 0; i < dimension; i++) {
                    if (i < embeddingArray.Length)
                        sumVector[i] += embeddingArray[i] * weight;
                }
                totalWeight += weight;
            }

            // Normalize by total weight if applicable
            if (totalWeight > 0) {
                for (int i = 0; i < dimension; i++) {
                    sumVector[i] /= totalWeight;
                }
            }

            // Normalize the vector (L2 normalization)
            float magnitude = (float)Math.Sqrt(sumVector.Sum(x => x * x));
            if (magnitude > 0) {
                for (int i = 0; i < dimension; i++) {
                    sumVector[i] /= magnitude;
                }
            }

            // Convert back to Vector type
            return new Vector(new ReadOnlyMemory<float>(sumVector));
        }

        // Keep the original AggregateEmbeddings method for backward compatibility
        private Vector AggregateEmbeddings(List<Vector> embeddings) {
            if (embeddings == null || embeddings.Count == 0)
                return null;

            int dimension = 384; // Based on the vector dimension in the Review model
            float[] sumVector = new float[dimension];

            // Sum all embeddings
            foreach (var embedding in embeddings) {
                if (embedding == null) continue;

                var embeddingArray = embedding.ToArray();
                for (int i = 0; i < dimension; i++) {
                    if (i < embeddingArray.Length)
                        sumVector[i] += embeddingArray[i];
                }
            }

            // Normalize the vector
            float magnitude = (float)Math.Sqrt(sumVector.Sum(x => x * x));
            if (magnitude > 0) {
                for (int i = 0; i < dimension; i++) {
                    sumVector[i] /= magnitude;
                }
            }

            // Convert back to Vector type
            return new Vector(new ReadOnlyMemory<float>(sumVector));
        }


        // DisplayedReview methods
        private async Task<List<Guid>> GetDisplayedReviewsForUserAsync(Guid profileId) {
            // Get reviews displayed in the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            return await _dbContext.DisplayedReviews
                .Where(dr => dr.ProfileId == profileId && dr.DisplayedAt >= thirtyDaysAgo)
                .Select(dr => dr.ReviewId)
                .ToListAsync();
        }

        private async Task AddDisplayedReviewsAsync(Guid profileId, List<Guid> reviewIds) {
            var displayedReviews = reviewIds.Select(reviewId => new DisplayedReview {
                ProfileId = profileId,
                ReviewId = reviewId,
                DisplayedAt = DateTime.UtcNow
            }).ToList();

            await _dbContext.DisplayedReviews.AddRangeAsync(displayedReviews);
            await _dbContext.SaveChangesAsync();
        }

        private async Task RemoveDisplayedReviewAsync(Guid profileId, Guid reviewId) {
            var displayedReview = await _dbContext.DisplayedReviews
                .FirstOrDefaultAsync(dr => dr.ProfileId == profileId && dr.ReviewId == reviewId);

            if (displayedReview != null) {
                _dbContext.DisplayedReviews.Remove(displayedReview);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
