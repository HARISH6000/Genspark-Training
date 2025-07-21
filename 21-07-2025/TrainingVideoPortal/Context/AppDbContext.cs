using Microsoft.EntityFrameworkCore;
using TrainingVideoPortal.Models;

namespace TrainingVideoPortal.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<TrainingVideo> TrainingVideos { get; set; }
    }
}
