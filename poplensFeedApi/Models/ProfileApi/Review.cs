namespace poplensUserProfileApi.Models {
    public class Review {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public float Rating { get; set; }
        public Guid ProfileId { get; set; }
        public string MediaId { get; set; } // Reference to the specific media item (movie, book, etc.)
        public DateTime CreatedDate { get; set; } // The date the review was created
        public DateTime LastUpdatedDate { get; set; } // The date the review was last updated
    }
}
