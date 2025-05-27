using System;
using System.Collections.Generic;

namespace Twitter.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Bio { get; set; }
        
        public ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();

    }
}