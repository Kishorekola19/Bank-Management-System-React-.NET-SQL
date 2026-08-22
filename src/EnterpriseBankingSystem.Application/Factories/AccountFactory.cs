using System;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;
using EnterpriseBankingSystem.Domain.Exceptions;

namespace EnterpriseBankingSystem.Application.Factories
{
    public interface IAccountFactory
    {
        Account CreateAccount(int customerId, AccountType accountType, decimal initialDeposit);
    }

    public class AccountFactory : IAccountFactory
    {
        public Account CreateAccount(int customerId, AccountType accountType, decimal initialDeposit)
        {
            if (customerId <= 0)
                throw new BankingDomainException("Valid Customer ID is required.");

            string accountNumber = GenerateAccountNumber(accountType);

            decimal interestRate;
            decimal overdraftLimit;

            switch (accountType)
            {
                case AccountType.Savings:
                    if (initialDeposit < 100m)
                        throw new BankingDomainException("Savings Account requires a minimum initial deposit of $100.");
                    interestRate = 4.5m;
                    overdraftLimit = 0m;
                    break;

                case AccountType.Checking:
                    if (initialDeposit < 50m)
                        throw new BankingDomainException("Checking Account requires a minimum initial deposit of $50.");
                    interestRate = 0.5m;
                    overdraftLimit = 500m; // Overdraft facility up to $500
                    break;

                case AccountType.FixedDeposit:
                    if (initialDeposit < 1000m)
                        throw new BankingDomainException("Fixed Deposit Account requires a minimum initial deposit of $1,000.");
                    interestRate = 7.0m;
                    overdraftLimit = 0m;
                    break;

                default:
                    throw new BankingDomainException($"Unsupported Account Type: {accountType}");
            }

            return new Account
            {
                CustomerId = customerId,
                AccountNumber = accountNumber,
                AccountType = accountType,
                Balance = initialDeposit,
                Currency = "USD",
                Status = AccountStatus.PendingApproval,
                InterestRate = interestRate,
                OverdraftLimit = overdraftLimit,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static string GenerateAccountNumber(AccountType accountType)
        {
            string prefix = accountType switch
            {
                AccountType.Savings => "SAV",
                AccountType.Checking => "CHK",
                AccountType.FixedDeposit => "FIX",
                _ => "ACC"
            };

            var random = new Random();
            long randomDigits = random.NextInt64(1000000000L, 9999999999L);
            return $"{prefix}-{randomDigits}";
        }
    }
}
