namespace poplensUserProfileApi.Models {
    public class Follow {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; } // Refers to the profile following another
        public Guid FollowingId { get; set; } // Refers to the profile being followed
        public DateTime CreatedDate { get; set; } // The date the follow was created
        public DateTime LastUpdatedDate { get; set; } // The date the follow was last updated
    }
}
