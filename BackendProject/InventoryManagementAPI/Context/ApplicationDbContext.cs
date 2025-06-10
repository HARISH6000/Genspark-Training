using Microsoft.EntityFrameworkCore;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products  { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryProduct> InventoryProducts { get; set; }
        public DbSet<InventoryManager> InventoryManagers { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            //Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired();

            //Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            //Role
            //RoleName
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .IsRequired();
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            //Category
            //CategoryName
            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired();
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();
            
            //Product
            //SKU
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();
            modelBuilder.Entity<Product>()
                .Property(p => p.SKU)
                .IsRequired();

            //ProductName
            modelBuilder.Entity<Product>()
                .Property(p => p.ProductName)
                .IsRequired();

            //Inventory
            //Location
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Location)
                .IsRequired();

            //Name
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Name)
                .IsRequired();
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.Name)
                .IsUnique();

            //InventoryManager
            modelBuilder.Entity<InventoryManager>()
                .HasKey(im => im.Id);
            modelBuilder.Entity<InventoryManager>()
                .HasIndex(im => new { im.InventoryId, im.ManagerId })
                .IsUnique();

            //InventoryProduct
            modelBuilder.Entity<InventoryProduct>()
                .HasKey(ip => ip.Id);
            modelBuilder.Entity<InventoryProduct>()
                .HasIndex(ip => new { ip.InventoryId, ip.ProductId })
                .IsUnique();

            //relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<InventoryProduct>()
                .HasOne(ip => ip.Inventory)
                .WithMany(i => i.InventoryProducts)
                .HasForeignKey(ip => ip.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<InventoryProduct>()
                .HasOne(ip => ip.Product)
                .WithMany(p => p.InventoryProducts)
                .HasForeignKey(ip => ip.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<InventoryManager>()
                .HasOne(im => im.Inventory)
                .WithMany(i => i.InventoryManagers)
                .HasForeignKey(im => im.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<InventoryManager>()
                .HasOne(im => im.Manager)
                .WithMany(u => u.ManagedInventories)
                .HasForeignKey(im => im.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

    }
}
