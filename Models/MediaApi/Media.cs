namespace poplensMediaApi.Models {
    public class Media {
        /// <summary>
        /// Our unique identifier
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Publish Date
        /// </summary>
        public DateTime PublishDate { get; set; }
        /// <summary>
        /// Genre in the format Action, Adventure, Comedy, etc.
        /// </summary>
        public string Genre { get; set; }
        /// <summary>
        /// External ID from the source API (check for duplicates with this)
        /// </summary>
        public string CachedExternalId { get; set; }
        /// <summary>
        /// Cached Image Path like /asd9121adssad (films)
        /// </summary>
        public string CachedImagePath { get; set; }
        /// <summary>
        /// Avg Rating from PopLens
        /// </summary>
        public double AvgRating { get; set; }
        /// <summary>
        /// Total Reviews from PopLens
        /// </summary>
        public int TotalReviews { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Determines if it's a film, book, or game
        /// </summary>
        public string Type { get; set; }

        // Specific to media types
        /// <summary>
        /// Director for films
        /// </summary>
        public string? Director { get; set; }
        /// <summary>
        /// Writer for books
        /// </summary>
        public string? Writer { get; set; } 
        /// <summary>
        /// Publisher for games
        /// </summary>
        public string? Publisher { get; set; }

        // Audit Fields
        /// <summary>
        /// Created Date for the db record
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// Last Updated Date for the db record
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }
    }

}
