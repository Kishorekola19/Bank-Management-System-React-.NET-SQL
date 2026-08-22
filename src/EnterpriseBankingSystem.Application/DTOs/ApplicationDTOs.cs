using System;
using System.ComponentModel.DataAnnotations;
using EnterpriseBankingSystem.Domain.Enums;

namespace EnterpriseBankingSystem.Application.DTOs
{
    // Auth DTOs
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // Customer DTOs
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string KycStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Account DTOs
    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";
        public AccountStatus Status { get; set; }
        public decimal InterestRate { get; set; }
        public decimal OverdraftLimit { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAccountDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        [Range(0, 10000000)]
        public decimal InitialDeposit { get; set; }
    }

    // ATM Card DTOs
    public class RequestAtmCardDto
    {
        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        public CardType CardType { get; set; } = CardType.VisaDebit;

        [Required, StringLength(4, MinimumLength = 4)]
        public string InitialPin { get; set; } = "1234";
    }

    public class AtmCardDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string MaskedCardNumber => string.IsNullOrWhiteSpace(CardNumber) ? "" : $"•••• •••• •••• {CardNumber.Substring(Math.Max(0, CardNumber.Length - 4))}";
        public string CardHolderName { get; set; } = string.Empty;
        public CardType CardType { get; set; }
        public string ExpiryDate { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public CardStatus Status { get; set; }
        public decimal DailyWithdrawalLimit { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class ChangePinDto
    {
        [Required, StringLength(4, MinimumLength = 4)]
        public string OldPin { get; set; } = string.Empty;

        [Required, StringLength(4, MinimumLength = 4)]
        public string NewPin { get; set; } = string.Empty;
    }

    public class UpdateCardStatusDto
    {
        [Required]
        public CardStatus Status { get; set; }
    }

    // Transaction DTOs
    public class DepositWithdrawDto
    {
        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    public class TransferDto
    {
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required]
        public string TargetAccountNumber { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string? TargetAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // Loan DTOs
    public class LoanApplicationDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public LoanType LoanType { get; set; }

        [Range(100, 5000000)]
        public decimal PrincipalAmount { get; set; }

        [Range(1, 360)]
        public int TenureMonths { get; set; }
    }

    public class LoanDto
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public LoanType LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public decimal RemainingAmount { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class LoanApprovalDto
    {
        public bool Approve { get; set; }
        public string ActionBy { get; set; } = "Admin";
    }

    public class LoanRepaymentDto
    {
        [Required]
        public int LoanId { get; set; }

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal RepaymentAmount { get; set; }
    }

    public class EmiCalculatorDto
    {
        public decimal PrincipalAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
    }

    public class EmiCalculatorResultDto
    {
        public decimal MonthlyEMI { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
    }

    // Audit Log DTO
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
