using NotifyAPI.Models;
using NotifyAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace NotifyAPI.Contexts
{
    public class NotifyContext : DbContext
    {

        public NotifyContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        
    }
}