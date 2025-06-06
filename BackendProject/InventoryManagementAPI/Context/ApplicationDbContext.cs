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

            //relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);


            modelBuilder.Entity<InventoryProduct>()
                .HasOne(ip => ip.Inventory)
                .WithMany(i => i.InventoryProducts)
                .HasForeignKey(ip => ip.InventoryID);


            modelBuilder.Entity<InventoryProduct>()
                .HasOne(ip => ip.Product)
                .WithMany(p => p.InventoryProducts)
                .HasForeignKey(ip => ip.ProductID);


            modelBuilder.Entity<InventoryManager>()
                .HasOne(im => im.Inventory)
                .WithMany(i => i.InventoryManagers)
                .HasForeignKey(im => im.InventoryID);


            modelBuilder.Entity<InventoryManager>()
                .HasOne(im => im.Manager)
                .WithMany(u => u.ManagedInventories)
                .HasForeignKey(im => im.ManagerId); 

            base.OnModelCreating(modelBuilder);
        }

    }
}
