using System;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;
using EnterpriseBankingSystem.Domain.Exceptions;

namespace EnterpriseBankingSystem.Application.Strategies
{
    public interface ITransactionStrategy
    {
        Task<Transaction> ExecuteAsync(IUnitOfWork unitOfWork, string sourceAccountNum, string? targetAccountNum, decimal amount, string description);
    }

    public class DepositTransactionStrategy : ITransactionStrategy
    {
        public async Task<Transaction> ExecuteAsync(IUnitOfWork unitOfWork, string sourceAccountNum, string? targetAccountNum, decimal amount, string description)
        {
            var accounts = await unitOfWork.Accounts.FindAsync(a => a.AccountNumber == sourceAccountNum);
            var account = System.Linq.Enumerable.FirstOrDefault(accounts);
            if (account == null)
                throw new AccountNotFoundException(sourceAccountNum);

            account.Deposit(amount);
            unitOfWork.Accounts.Update(account);

            var transaction = new Transaction
            {
                SourceAccountId = account.Id,
                Amount = amount,
                FeeAmount = 0m,
                TransactionType = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = string.IsNullOrWhiteSpace(description) ? "Account Deposit" : description,
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.Transactions.AddAsync(transaction);
            return transaction;
        }
    }

    public class WithdrawalTransactionStrategy : ITransactionStrategy
    {
        public async Task<Transaction> ExecuteAsync(IUnitOfWork unitOfWork, string sourceAccountNum, string? targetAccountNum, decimal amount, string description)
        {
            var accounts = await unitOfWork.Accounts.FindAsync(a => a.AccountNumber == sourceAccountNum);
            var account = System.Linq.Enumerable.FirstOrDefault(accounts);
            if (account == null)
                throw new AccountNotFoundException(sourceAccountNum);

            account.Withdraw(amount);
            unitOfWork.Accounts.Update(account);

            var transaction = new Transaction
            {
                SourceAccountId = account.Id,
                Amount = amount,
                FeeAmount = 0m,
                TransactionType = TransactionType.Withdrawal,
                Status = TransactionStatus.Completed,
                Description = string.IsNullOrWhiteSpace(description) ? "Account Withdrawal" : description,
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.Transactions.AddAsync(transaction);
            return transaction;
        }
    }

    public class TransferTransactionStrategy : ITransactionStrategy
    {
        public async Task<Transaction> ExecuteAsync(IUnitOfWork unitOfWork, string sourceAccountNum, string? targetAccountNum, decimal amount, string description)
        {
            if (string.IsNullOrWhiteSpace(targetAccountNum))
                throw new BankingDomainException("Target account number is required for transfer.");

            if (sourceAccountNum == targetAccountNum)
                throw new BankingDomainException("Source and target account numbers cannot be identical.");

            var srcList = await unitOfWork.Accounts.FindAsync(a => a.AccountNumber == sourceAccountNum);
            var srcAccount = System.Linq.Enumerable.FirstOrDefault(srcList);
            if (srcAccount == null)
                throw new AccountNotFoundException(sourceAccountNum);

            var tgtList = await unitOfWork.Accounts.FindAsync(a => a.AccountNumber == targetAccountNum);
            var tgtAccount = System.Linq.Enumerable.FirstOrDefault(tgtList);
            if (tgtAccount == null)
                throw new AccountNotFoundException(targetAccountNum);

            // ACID atomic transfer execution
            srcAccount.Withdraw(amount);
            tgtAccount.Deposit(amount);

            unitOfWork.Accounts.Update(srcAccount);
            unitOfWork.Accounts.Update(tgtAccount);

            var transaction = new Transaction
            {
                SourceAccountId = srcAccount.Id,
                TargetAccountId = tgtAccount.Id,
                Amount = amount,
                FeeAmount = 0m,
                TransactionType = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = string.IsNullOrWhiteSpace(description) ? $"Transfer to {targetAccountNum}" : description,
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.Transactions.AddAsync(transaction);
            return transaction;
        }
    }
}
