using Twitter.Models;
using Microsoft.EntityFrameworkCore;

namespace Twitter.Contexts
{
    public class TwitterContext : DbContext
    {

        public TwitterContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<TweetHashtag> TweetHashtags { get; set; }
        public DbSet<Follow> Follows { get; set; }

    }
}