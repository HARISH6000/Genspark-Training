namespace FirstAPI.Models
{


    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int FollwerId { get; set; }
        public ICollection<User>? Followers { get; set; }
        public User? UserFollower{ get; set; }
    }
}