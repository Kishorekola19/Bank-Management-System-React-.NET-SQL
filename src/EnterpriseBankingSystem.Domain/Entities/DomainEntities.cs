using System;
using System.Collections.Generic;
using EnterpriseBankingSystem.Domain.Enums;
using EnterpriseBankingSystem.Domain.Exceptions;

namespace EnterpriseBankingSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string KycStatus { get; set; } = "Verified";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }

    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public decimal InterestRate { get; set; }
        public decimal OverdraftLimit { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public byte[]? RowVersion { get; set; }

        public ICollection<AtmCard> AtmCards { get; set; } = new List<AtmCard>();

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new BankingDomainException("Deposit amount must be greater than zero.");
            
            if (Status != AccountStatus.Active)
                throw new BankingDomainException($"Cannot deposit to account in status: {Status}");

            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new BankingDomainException("Withdrawal amount must be greater than zero.");

            if (Status != AccountStatus.Active)
                throw new BankingDomainException($"Cannot withdraw from account in status: {Status}");

            decimal availableLimit = Balance + OverdraftLimit;
            if (amount > availableLimit)
                throw new InsufficientFundsException(AccountNumber, availableLimit, amount);

            Balance -= amount;
        }
    }

    public class AtmCard
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public CardType CardType { get; set; } = CardType.VisaDebit;
        public string ExpiryDate { get; set; } = string.Empty; // MM/YY
        public string Cvv { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public CardStatus Status { get; set; } = CardStatus.Active;
        public decimal DailyWithdrawalLimit { get; set; } = 2000.00m;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }

    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        public int SourceAccountId { get; set; }
        public Account? SourceAccount { get; set; }

        public int? TargetAccountId { get; set; }
        public Account? TargetAccount { get; set; }

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class Loan
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public LoanType LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public decimal RemainingAmount { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Pending;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string Username { get; set; } = "System";
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "127.0.0.1";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
