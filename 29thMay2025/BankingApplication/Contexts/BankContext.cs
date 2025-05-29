using BankingApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Contexts
{
    public class BankContext : DbContext
    {

        public BankContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Account>().HasIndex(a => a.AccountNumber).IsUnique();

            modelBuilder.Entity<Account>().HasOne(a => a.User)
                                        .WithMany()
                                        .HasForeignKey(a => a.UserId) 
                                        .HasConstraintName("FK_Account_User")
                                        .IsRequired();

            modelBuilder.Entity<Transaction>().HasIndex(t => t.ReferenceNumber).IsUnique();

            modelBuilder.Entity<Transaction>().HasOne<Account>()
                                            .WithMany()
                                            .HasForeignKey(t => t.AccountId);

            modelBuilder.Entity<Transaction>().HasOne<Account>()
                                            .WithMany()
                                            .HasForeignKey(t => t.TargetAccountId)
                                            .IsRequired(false);

        }

    }
}