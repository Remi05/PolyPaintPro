namespace PolyPaint.Models
{
    public class User
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }

        public User(string id, string displayName, string email, string photoUrl)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            PhotoUrl = photoUrl;
        }
    }
}