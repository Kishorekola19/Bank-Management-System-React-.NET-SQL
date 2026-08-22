namespace EnterpriseBankingSystem.Domain.Enums
{
    public enum UserRole
    {
        Customer = 1,
        Teller = 2,
        Admin = 3
    }

    public enum AccountType
    {
        Savings = 1,
        Checking = 2,
        FixedDeposit = 3
    }

    public enum AccountStatus
    {
        PendingApproval = 1,
        Active = 2,
        PendingClosure = 3,
        Closed = 4,
        Frozen = 5,
        Rejected = 6
    }

    public enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2,
        Transfer = 3,
        LoanRepayment = 4,
        InterestCredit = 5
    }

    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3
    }

    public enum LoanStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Active = 4,
        Closed = 5
    }

    public enum LoanType
    {
        Personal = 1,
        Home = 2,
        Auto = 3,
        Business = 4
    }

    public enum CardStatus
    {
        PendingApproval = 1,
        Active = 2,
        Blocked = 3,
        Expired = 4,
        Rejected = 5
    }

    public enum CardType
    {
        VisaDebit = 1,
        MastercardDebit = 2,
        PlatinumRuPay = 3
    }
}
