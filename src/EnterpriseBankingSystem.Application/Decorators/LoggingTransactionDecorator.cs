using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.DTOs;
using EnterpriseBankingSystem.Application.Services;

namespace EnterpriseBankingSystem.Application.Decorators
{
    public class LoggingTransactionDecorator : ITransactionService
    {
        private readonly ITransactionService _innerService;
        private readonly IAuditLogger _auditLogger;

        public LoggingTransactionDecorator(ITransactionService innerService, IAuditLogger auditLogger)
        {
            _innerService = innerService;
            _auditLogger = auditLogger;
        }

        public async Task<TransactionDto> DepositAsync(DepositWithdrawDto dto, string performedBy = "User")
        {
            var result = await _innerService.DepositAsync(dto, performedBy);
            await _auditLogger.LogAsync(performedBy, "Deposit", "Account", dto.AccountNumber, $"Deposited ${dto.Amount} to account {dto.AccountNumber}. Ref: {result.TransactionReference}");
            return result;
        }

        public async Task<TransactionDto> WithdrawAsync(DepositWithdrawDto dto, string performedBy = "User")
        {
            var result = await _innerService.WithdrawAsync(dto, performedBy);
            await _auditLogger.LogAsync(performedBy, "Withdrawal", "Account", dto.AccountNumber, $"Withdrew ${dto.Amount} from account {dto.AccountNumber}. Ref: {result.TransactionReference}");
            return result;
        }

        public async Task<TransactionDto> TransferAsync(TransferDto dto, string performedBy = "User")
        {
            var result = await _innerService.TransferAsync(dto, performedBy);
            await _auditLogger.LogAsync(performedBy, "Transfer", "Account", dto.SourceAccountNumber, $"Transferred ${dto.Amount} from {dto.SourceAccountNumber} to {dto.TargetAccountNumber}. Ref: {result.TransactionReference}");
            return result;
        }

        public Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(string accountNumber)
        {
            return _innerService.GetAccountTransactionsAsync(accountNumber);
        }

        public Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            return _innerService.GetAllTransactionsAsync();
        }
    }
}
