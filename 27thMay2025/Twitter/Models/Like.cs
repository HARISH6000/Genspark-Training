using System;

namespace Twitter.Models
{
    public class Like
    {
        public int Id{ get; set; }
        public int UserId { get; set; }
        public int TweetId { get; set; } 
        public DateTime LikedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = default!;
        public Tweet Tweet { get; set; } = default!;
    }
}