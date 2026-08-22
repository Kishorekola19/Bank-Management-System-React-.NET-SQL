using System;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseBankingSystem.Infrastructure.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<AtmCard> AtmCards => Set<AtmCard>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(150);
            });

            // Customer Configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            });

            // Account Configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.AccountNumber).IsUnique();
                entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(30);
                entity.Property(a => a.Balance).HasPrecision(18, 2);
                entity.Property(a => a.OverdraftLimit).HasPrecision(18, 2);
                entity.Property(a => a.InterestRate).HasPrecision(5, 2);

                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Accounts)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AtmCard Configuration
            modelBuilder.Entity<AtmCard>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.CardNumber).IsUnique();
                entity.Property(c => c.CardNumber).IsRequired().HasMaxLength(25);
                entity.Property(c => c.DailyWithdrawalLimit).HasPrecision(18, 2);

                entity.HasOne(c => c.Account)
                      .WithMany(a => a.AtmCards)
                      .HasForeignKey(c => c.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Transaction Configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.TransactionReference).IsUnique();
                entity.Property(t => t.Amount).HasPrecision(18, 2);
                entity.Property(t => t.FeeAmount).HasPrecision(18, 2);

                entity.HasOne(t => t.SourceAccount)
                      .WithMany()
                      .HasForeignKey(t => t.SourceAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.TargetAccount)
                      .WithMany()
                      .HasForeignKey(t => t.TargetAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Loan Configuration
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => l.LoanNumber).IsUnique();
                entity.Property(l => l.PrincipalAmount).HasPrecision(18, 2);
                entity.Property(l => l.MonthlyEMI).HasPrecision(18, 2);
                entity.Property(l => l.RemainingAmount).HasPrecision(18, 2);

                entity.HasOne(l => l.Customer)
                      .WithMany(c => c.Loans)
                      .HasForeignKey(l => l.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    FirstName = "Kishore",
                    LastName = "Kola",
                    Email = "kishore@admin.bank.com",
                    Phone = "+1-800-555-0199",
                    Address = "100 Financial Center, New York, NY",
                    KycStatus = "Verified",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Customer
                {
                    Id = 2,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@bank.com",
                    Phone = "+1-555-0123",
                    Address = "123 Financial Way, New York, NY",
                    KycStatus = "Verified",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Customer
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@bank.com",
                    Phone = "+1-555-0456",
                    Address = "456 Commerce Blvd, New York, NY",
                    KycStatus = "Verified",
                    CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            string adminHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Kishore19@")));
            string userHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("User@123")));
            string pinHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("1234")));

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "Kishore",
                    Email = "kishore@admin.bank.com",
                    PasswordHash = adminHash,
                    FullName = "Kishore Kola (Admin)",
                    PhoneNumber = "+1-800-555-0199",
                    Role = UserRole.Admin,
                    CustomerId = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "john.doe@bank.com",
                    Email = "john.doe@bank.com",
                    PasswordHash = userHash,
                    FullName = "John Doe",
                    PhoneNumber = "+1-555-0123",
                    Role = UserRole.Customer,
                    CustomerId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "jane.smith@bank.com",
                    Email = "jane.smith@bank.com",
                    PasswordHash = userHash,
                    FullName = "Jane Smith",
                    PhoneNumber = "+1-555-0456",
                    Role = UserRole.Customer,
                    CustomerId = 3,
                    CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = 1,
                    AccountNumber = "SAV-8839201948",
                    CustomerId = 2,
                    AccountType = AccountType.Savings,
                    Balance = 25000.00m,
                    Currency = "USD",
                    Status = AccountStatus.Active,
                    InterestRate = 4.5m,
                    OverdraftLimit = 0m,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Account
                {
                    Id = 2,
                    AccountNumber = "CHK-9182746352",
                    CustomerId = 2,
                    AccountType = AccountType.Checking,
                    Balance = 8500.50m,
                    Currency = "USD",
                    Status = AccountStatus.Active,
                    InterestRate = 0.5m,
                    OverdraftLimit = 500m,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Account
                {
                    Id = 3,
                    AccountNumber = "SAV-1029384756",
                    CustomerId = 3,
                    AccountType = AccountType.Savings,
                    Balance = 12400.00m,
                    Currency = "USD",
                    Status = AccountStatus.Active,
                    InterestRate = 4.5m,
                    OverdraftLimit = 0m,
                    CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<AtmCard>().HasData(
                new AtmCard
                {
                    Id = 1,
                    AccountId = 1,
                    CardNumber = "4532 8819 2039 4812",
                    CardHolderName = "JOHN DOE",
                    CardType = CardType.VisaDebit,
                    ExpiryDate = "12/28",
                    Cvv = "892",
                    PinHash = pinHash,
                    Status = CardStatus.Active,
                    DailyWithdrawalLimit = 2500.00m,
                    IssuedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
                },
                new AtmCard
                {
                    Id = 2,
                    AccountId = 2,
                    CardNumber = "5412 9182 7463 5290",
                    CardHolderName = "JOHN DOE",
                    CardType = CardType.MastercardDebit,
                    ExpiryDate = "09/29",
                    Cvv = "415",
                    PinHash = pinHash,
                    Status = CardStatus.Active,
                    DailyWithdrawalLimit = 3000.00m,
                    IssuedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new AtmCard
                {
                    Id = 3,
                    AccountId = 3,
                    CardNumber = "6011 1029 3847 5601",
                    CardHolderName = "JANE SMITH",
                    CardType = CardType.PlatinumRuPay,
                    ExpiryDate = "05/29",
                    Cvv = "118",
                    PinHash = pinHash,
                    Status = CardStatus.Active,
                    DailyWithdrawalLimit = 5000.00m,
                    IssuedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Loan>().HasData(
                new Loan
                {
                    Id = 1,
                    LoanNumber = "LN-88192034",
                    CustomerId = 2,
                    LoanType = LoanType.Personal,
                    PrincipalAmount = 15000.00m,
                    InterestRate = 11.5m,
                    TenureMonths = 24,
                    MonthlyEMI = 702.73m,
                    RemainingAmount = 14297.27m,
                    Status = LoanStatus.Active,
                    AppliedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    ApprovedAt = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
                    ApprovedBy = "Kishore Kola (Admin)"
                }
            );
        }
    }
}
