using System;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;

namespace EnterpriseBankingSystem.Application.Strategies
{
    public interface IInterestCalculatorStrategy
    {
        decimal CalculateAnnualInterest(Account account);
    }

    public class SavingsInterestStrategy : IInterestCalculatorStrategy
    {
        public decimal CalculateAnnualInterest(Account account)
        {
            // Savings account gets 4.5% standard interest, bonus 0.5% for balances > $10,000
            decimal rate = account.Balance >= 10000m ? 0.05m : 0.045m;
            return Math.Round(account.Balance * rate, 2);
        }
    }

    public class CheckingInterestStrategy : IInterestCalculatorStrategy
    {
        public decimal CalculateAnnualInterest(Account account)
        {
            // Checking account gets 0.5% nominal interest
            return Math.Round(account.Balance * 0.005m, 2);
        }
    }

    public class FixedDepositInterestStrategy : IInterestCalculatorStrategy
    {
        public decimal CalculateAnnualInterest(Account account)
        {
            // Fixed Deposit gets 7.0% annual interest
            return Math.Round(account.Balance * 0.07m, 2);
        }
    }

    public interface ILoanApprovalStrategy
    {
        (bool IsApproved, string Reason, decimal DefaultInterestRate) EvaluateApplication(Customer customer, LoanType loanType, decimal principalAmount, int tenureMonths);
    }

    public class DefaultLoanApprovalStrategy : ILoanApprovalStrategy
    {
        public (bool IsApproved, string Reason, decimal DefaultInterestRate) EvaluateApplication(Customer customer, LoanType loanType, decimal principalAmount, int tenureMonths)
        {
            if (customer.KycStatus != "Verified")
            {
                return (false, "KYC status must be Verified to apply for loans.", 0m);
            }

            if (principalAmount <= 0 || tenureMonths <= 0)
            {
                return (false, "Invalid principal amount or tenure months.", 0m);
            }

            decimal interestRate = loanType switch
            {
                LoanType.Personal => 11.5m,
                LoanType.Home => 7.5m,
                LoanType.Auto => 8.5m,
                LoanType.Business => 10.0m,
                _ => 12.0m
            };

            // Basic risk scoring check: maximum allowed personal loan limit $100,000 without collateral
            if (loanType == LoanType.Personal && principalAmount > 100000m)
            {
                return (false, "Personal loan amount exceeds the non-collateral limit of $100,000.", interestRate);
            }

            return (true, "Loan application met all risk assessment criteria.", interestRate);
        }
    }
}
