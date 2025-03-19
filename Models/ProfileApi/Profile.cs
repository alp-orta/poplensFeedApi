namespace poplensUserProfileApi.Models {
    public class Profile {
        public Guid Id { get; set; }
        public string UserId { get; set; } // This will map to the Authentication API's User ID
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Follow> Followers { get; set; } = new List<Follow>();
        public List<Follow> Following { get; set; } = new List<Follow>();
    }
}
