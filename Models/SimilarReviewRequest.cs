namespace poplensFeedApi.Models.Feed {
    public class SimilarReviewRequest {
        public float[] Embedding { get; set; }
        public int Count { get; set; } = 10;
        public List<Guid>? ExcludedReviewIds { get; set; }
        public Guid? RequestingProfileId { get; set; }
    }


}
