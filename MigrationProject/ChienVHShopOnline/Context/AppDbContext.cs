using Microsoft.EntityFrameworkCore;
using ChienVHShopOnline.Models;

namespace ChienVHShopOnline.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Model> Models { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ContactU> ContactUs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite primary key for OrderDetail
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderID, od.ProductID });

        // Optional: Add relationships and constraints as needed
        base.OnModelCreating(modelBuilder);
    }
}
