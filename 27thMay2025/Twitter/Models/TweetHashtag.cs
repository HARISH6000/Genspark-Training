using System;

namespace Twitter.Models
{
    public class TweetHashtag
    {
        public int Id{ get; set; }
        public int TweetId { get; set; } 
        public int HashtagId { get; set; }
        public Tweet Tweet { get; set; } = default!;
        public Hashtag Hashtag { get; set; } = default!;
    }
}