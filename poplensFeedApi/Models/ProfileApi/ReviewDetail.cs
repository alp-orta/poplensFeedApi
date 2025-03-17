namespace poplensUserProfileApi.Models {
    public class ReviewDetail {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public float Rating { get; set; }
        public Guid ProfileId { get; set; }
        public string MediaId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string MediaTitle { get; set; }
        public string MediaType { get; set; }
        public string MediaCachedImagePath { get; set; }
        public string MediaCreator { get; set; }

    }
}
