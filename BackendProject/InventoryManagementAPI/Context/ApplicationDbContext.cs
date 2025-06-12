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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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


            //AuditLog
            modelBuilder.Entity<AuditLog>()
                .HasKey(al => al.AuditLogId);
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.Timestamp)
                .IsRequired();
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.TableName)
                .IsRequired();
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.RecordId)
                .IsRequired();
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.ActionType)
                .IsRequired();
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.OldValues)
                .IsRequired(false);
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.NewValues)
                .IsRequired(false);
            modelBuilder.Entity<AuditLog>()
                .Property(al => al.Changes)
                .IsRequired(false);

            //RevokedToken
            modelBuilder.Entity<RevokedToken>()
                .HasKey(rt => rt.Jti);

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

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

    }
}
