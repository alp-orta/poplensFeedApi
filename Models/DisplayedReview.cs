using System.ComponentModel.DataAnnotations;

namespace poplensFeedApi.Models {
    public class DisplayedReview {
        [Key]
        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }

        public Guid ReviewId { get; set; }

        public DateTime DisplayedAt { get; set; }
    }
}
