using System;

namespace EnterpriseBankingSystem.Domain.Exceptions
{
    public class BankingDomainException : Exception
    {
        public BankingDomainException(string message) : base(message) { }
        public BankingDomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InsufficientFundsException : BankingDomainException
    {
        public InsufficientFundsException(string accountNumber, decimal available, decimal requested)
            : base($"Account {accountNumber} has insufficient funds. Available: {available:C}, Requested: {requested:C}") { }
    }

    public class AccountNotFoundException : BankingDomainException
    {
        public AccountNotFoundException(string identifier)
            : base($"Account '{identifier}' was not found.") { }
    }

    public class LoanNotFoundException : BankingDomainException
    {
        public LoanNotFoundException(int loanId)
            : base($"Loan with ID {loanId} was not found.") { }
    }
}
