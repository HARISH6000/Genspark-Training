using System;
using System.Collections.Generic;

namespace Twitter.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User User { get; set; } = default!; 
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();
    }
}