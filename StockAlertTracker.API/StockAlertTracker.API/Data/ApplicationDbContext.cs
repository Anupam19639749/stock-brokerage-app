using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using System.Security.Cryptography; // For password hashing
using System.Text;

namespace StockAlertTracker.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define all our database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PortfolioHolding> PortfolioHoldings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<PriceAlert> PriceAlerts { get; set; }
        public DbSet<PlatformStats> PlatformStats { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configure Relationships and Constraints ---

            // User Entity
            modelBuilder.Entity<User>(entity =>
            {
                // Create a unique index on the Email column
                entity.HasIndex(u => u.Email).IsUnique();

                // Store enums as strings (e.g., "Admin") instead of numbers (e.g., 1)
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                entity.Property(u => u.KycStatus).HasConversion<string>().HasMaxLength(20);

                // Configure the 1-to-1 relationship between User and Wallet
                entity.HasOne(u => u.Wallet)
                      .WithOne(w => w.User)
                      .HasForeignKey<Wallet>(w => w.UserId) // The FK is on the Wallet table
                      .OnDelete(DeleteBehavior.Cascade);    // Delete wallet if user is deleted

                // Configure 1-to-Many relationships
                entity.HasMany(u => u.PortfolioHoldings)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.PriceAlerts)
                      .WithOne(a => a.User)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.PasswordResetTokens)
                      .WithOne(t => t.User)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Wallet Entity
            modelBuilder.Entity<Wallet>(entity =>
            {
                // Configure 1-to-Many relationship with WalletTransactions
                entity.HasMany(w => w.WalletTransactions)
                      .WithOne(t => t.Wallet)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete transactions if wallet is deleted
            });

            // WalletTransaction Entity
            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
            });

            // Order Entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            });

            // PriceAlert Entity
            modelBuilder.Entity<PriceAlert>(entity =>
            {
                entity.Property(a => a.Condition).HasConversion<string>().HasMaxLength(20);
                entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            });

            // --- Seed the Admin User ---
            SeedAdminUser(modelBuilder);
        }

        private void SeedAdminUser(ModelBuilder modelBuilder)
        {
            // We need to pre-generate the hash and salt for our password
            // HasData runs at migration time, so the values must be static

            // Password: "Admin@123"
            // Below is the hash & salt generated from the helper function
            byte[] adminPasswordHash = new byte[]
            {
                0x84, 0xC2, 0xD9, 0xD9, 0x89, 0xC8, 0x5D, 0x41, 0xA5, 0x3F, 0x47, 0xF1, 0x4D, 0xC5, 0x5D, 0xF0,
                0x73, 0x4D, 0x3B, 0x24, 0x1B, 0x7A, 0xF6, 0x1A, 0x05, 0x54, 0x0C, 0xF7, 0x7E, 0xC7, 0x47, 0xF1,
                0x0E, 0x98, 0xC4, 0x2C, 0xC7, 0xB6, 0xDB, 0x58, 0xE7, 0x12, 0x55, 0x5C, 0xA2, 0x70, 0x93, 0x8C,
                0xC1, 0xA5, 0x1A, 0x4E, 0x58, 0x44, 0xA2, 0x82, 0xFB, 0xED, 0x9E, 0x26, 0x72, 0x1E, 0x89, 0x5B
            };

            byte[] adminPasswordSalt = new byte[]
            {
                0xE4, 0x7E, 0xE2, 0x4B, 0x29, 0xCE, 0x76, 0x4A, 0x9E, 0x08, 0xC6, 0x4D, 0xFC, 0x7F, 0x47, 0xF9,
                0xC5, 0x6E, 0x95, 0xF7, 0xF8, 0xC4, 0x16, 0x6E, 0x4C, 0xDF, 0xF3, 0xC8, 0x84, 0x4A, 0x60, 0xC2,
                0x36, 0xE4, 0x97, 0xC5, 0xF3, 0x0D, 0xE9, 0xF9, 0xA0, 0x1B, 0x59, 0x9A, 0x31, 0xC0, 0x7F, 0x09,
                0x7A, 0x35, 0x60, 0xC4, 0x44, 0xD1, 0x3E, 0xB7, 0xA5, 0x06, 0x4A, 0x4C, 0x0E, 0x15, 0x5E, 0xF5,
                0x57, 0x8A, 0x2E, 0x0B, 0x0D, 0xAE, 0xCF, 0xE0, 0x54, 0x02, 0x45, 0x00, 0xB6, 0xC6, 0x9B, 0xC4,
                0x9A, 0xFD, 0x44, 0x4B, 0x89, 0xA1, 0xC3, 0x86, 0x04, 0xDE, 0xF2, 0xC9, 0x7F, 0xC8, 0xC3, 0x1D,
                0x4B, 0x56, 0x2E, 0x0A, 0x4D, 0x95, 0x89, 0x07, 0xA0, 0x64, 0x77, 0xB3, 0xF6, 0x81, 0x62, 0x04,
                0x59, 0xD7, 0xF0, 0xA8, 0xF4, 0x24, 0x2C, 0x4C, 0xB5, 0xFD, 0xC9, 0x99, 0xE7, 0x91, 0x2C, 0x57
            };


            // Seed the Admin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1, // First user, so ID is 1
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@admin.com",
                PhoneNumber = "1234567890",
                PasswordHash = adminPasswordHash,
                PasswordSalt = adminPasswordSalt,
                Gender = null,
                DateOfBirth = null,
                ProfileImage = null,
                PanNumber = "ADMIN0000A",
                BankName = "Admin Bank",
                BankAccountNumber = "0000000000",
                BankIfscCode = "ADMIN000001",
                KycStatus = KycStatus.Approved, // Admin is auto-approved
                LastLogin = null,
                CreatedAt = new DateTime(2025, 1, 1),
                Role = RoleType.Admin
            });
        }
    }
}