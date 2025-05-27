using System;
using System.Collections.Generic;

namespace Twitter.Models
{
    public class Hashtag
    {
        public int Id { get; set; }
        public string TagText { get; set; } = string.Empty; 

        public ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();
    }
}