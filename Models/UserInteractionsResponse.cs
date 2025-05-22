using poplensUserProfileApi.Models;

namespace poplensFeedApi.Models {
    public class UserInteractionsResponse {
        public List<Review> OwnReviews { get; set; } = new List<Review>();
        public List<Review> LikedReviews { get; set; } = new List<Review>();
        public List<Review> CommentedReviews { get; set; } = new List<Review>();
    }
}
