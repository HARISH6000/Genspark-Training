using System;

namespace Twitter.Models
{
    public class Follow
    {
        public int Id{ get; set; }
        public int FollowerId { get; set; }
        public int FolloweeId { get; set; }
        public DateTime FollowedAt { get; set; } = DateTime.Now;

        
        public User Follower { get; set; } = default!; 
        public User Followee { get; set; } = default!;
    }
}