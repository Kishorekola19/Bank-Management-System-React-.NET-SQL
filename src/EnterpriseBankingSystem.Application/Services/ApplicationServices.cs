using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.DTOs;
using EnterpriseBankingSystem.Application.Factories;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Application.Strategies;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;
using EnterpriseBankingSystem.Domain.Exceptions;

namespace EnterpriseBankingSystem.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    }

    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> CreateAsync(RegisterRequestDto request);
    }

    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(CreateAccountDto request);
        Task<AccountDto?> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<AccountDto>> GetCustomerAccountsAsync(int customerId);
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
        Task<IEnumerable<AccountDto>> GetPendingAccountRequestsAsync();
        Task<AccountDto> ApproveAccountAsync(int accountId, bool approve, string adminUsername);
        Task<AccountDto> RequestAccountClosureAsync(string accountNumber, string requestedBy);
        Task<AccountDto> ApproveAccountClosureAsync(int accountId, bool approve, string adminUsername);
        Task<decimal> CalculateAnnualInterestAsync(string accountNumber);
        Task<bool> DeleteAccountAsync(string accountNumber);
    }

    public interface IAtmCardService
    {
        Task<AtmCardDto> RequestCardAsync(RequestAtmCardDto request, string performedBy = "User");
        Task<IEnumerable<AtmCardDto>> GetCustomerCardsAsync(int customerId);
        Task<IEnumerable<AtmCardDto>> GetAccountCardsAsync(string accountNumber);
        Task<IEnumerable<AtmCardDto>> GetAllCardsAsync();
        Task<IEnumerable<AtmCardDto>> GetPendingCardsAsync();
        Task<AtmCardDto> ApproveCardAsync(int cardId, bool approve, string adminUsername);
        Task<bool> ChangePinAsync(int cardId, ChangePinDto pinDto);
        Task<AtmCardDto> UpdateCardStatusAsync(int cardId, CardStatus status, string performedBy = "Admin");
    }

    public interface ITransactionService
    {
        Task<TransactionDto> DepositAsync(DepositWithdrawDto dto, string performedBy = "User");
        Task<TransactionDto> WithdrawAsync(DepositWithdrawDto dto, string performedBy = "User");
        Task<TransactionDto> TransferAsync(TransferDto dto, string performedBy = "User");
        Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(string accountNumber);
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
    }

    public interface ILoanService
    {
        Task<LoanDto> ApplyLoanAsync(LoanApplicationDto request);
        Task<LoanDto> ProcessLoanApprovalAsync(int loanId, LoanApprovalDto approvalDto);
        Task<TransactionDto> RepayEmiAsync(LoanRepaymentDto repaymentDto);
        Task<IEnumerable<LoanDto>> GetCustomerLoansAsync(int customerId);
        Task<IEnumerable<LoanDto>> GetAllLoansAsync();
        EmiCalculatorResultDto CalculateEmi(EmiCalculatorDto dto);
    }

    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }

    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }

    public interface IAuditLogger
    {
        Task LogAsync(string username, string action, string entityName, string entityId, string details, string ipAddress = "127.0.0.1");
    }

    // AuthService Implementation
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IAuditLogger _auditLogger;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IAuditLogger auditLogger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _auditLogger = auditLogger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var reqKey = (request.Email ?? string.Empty).ToLower();
            var users = await _unitOfWork.Users.FindAsync(u => u.Username.ToLower() == reqKey || u.Email.ToLower() == reqKey);
            var user = users.FirstOrDefault();
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new BankingDomainException("Invalid username/email or password.");
            }

            var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);
            await _auditLogger.LogAsync(user.Username, "UserLogin", "User", user.Id.ToString(), "User logged in successfully.");

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                CustomerId = user.CustomerId,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existingUsers.Any())
            {
                throw new BankingDomainException($"User with email {request.Email} already exists.");
            }

            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                KycStatus = "Verified",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            var user = new User
            {
                Username = request.Email,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                FullName = $"{request.FirstName} {request.LastName}",
                PhoneNumber = request.Phone,
                Role = request.Role,
                CustomerId = customer.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);
            await _auditLogger.LogAsync(user.Username, "UserRegister", "User", user.Id.ToString(), $"Registered user {user.Email} with role {user.Role}.");

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                CustomerId = customer.Id,
                ExpiresAt = expiresAt
            };
        }
    }

    // CustomerService Implementation
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var c = await _unitOfWork.Customers.GetByIdAsync(id);
            if (c == null) return null;
            return MapToDto(c);
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return customers.Select(MapToDto);
        }

        public async Task<CustomerDto> CreateAsync(RegisterRequestDto request)
        {
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                KycStatus = "Verified"
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();
            return MapToDto(customer);
        }

        private static CustomerDto MapToDto(Customer c) => new CustomerDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            KycStatus = c.KycStatus,
            CreatedAt = c.CreatedAt
        };
    }

    // AccountService Implementation
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountFactory _accountFactory;
        private readonly IAuditLogger _auditLogger;

        public AccountService(IUnitOfWork unitOfWork, IAccountFactory accountFactory, IAuditLogger auditLogger)
        {
            _unitOfWork = unitOfWork;
            _accountFactory = accountFactory;
            _auditLogger = auditLogger;
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountDto request)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new BankingDomainException($"Customer with ID {request.CustomerId} does not exist.");

            var account = _accountFactory.CreateAccount(request.CustomerId, request.AccountType, request.InitialDeposit);
            await _unitOfWork.Accounts.AddAsync(account);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync(customer.Email, "AccountCreate", "Account", account.AccountNumber, $"Created {account.AccountType} account with initial deposit ${account.Balance}.");

            return MapToDto(account, customer.FullName);
        }

        public async Task<AccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var list = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = list.FirstOrDefault();
            if (account == null) return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(account.CustomerId);
            return MapToDto(account, customer?.FullName ?? "Unknown");
        }

        public async Task<IEnumerable<AccountDto>> GetCustomerAccountsAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            var accounts = await _unitOfWork.Accounts.FindAsync(a => a.CustomerId == customerId);
            return accounts.Select(a => MapToDto(a, customer?.FullName ?? "Customer"));
        }

        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _unitOfWork.Accounts.GetAllAsync();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToDictionary(c => c.Id, c => c.FullName);
            return accounts.Select(a => MapToDto(a, customers.TryGetValue(a.CustomerId, out var name) ? name : "Customer"));
        }

        public async Task<decimal> CalculateAnnualInterestAsync(string accountNumber)
        {
            var list = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = list.FirstOrDefault() ?? throw new AccountNotFoundException(accountNumber);

            IInterestCalculatorStrategy strategy = account.AccountType switch
            {
                AccountType.Savings => new SavingsInterestStrategy(),
                AccountType.Checking => new CheckingInterestStrategy(),
                AccountType.FixedDeposit => new FixedDepositInterestStrategy(),
                _ => new SavingsInterestStrategy()
            };

            return strategy.CalculateAnnualInterest(account);
        }

        public async Task<bool> DeleteAccountAsync(string accountNumber)
        {
            var list = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = list.FirstOrDefault() ?? throw new AccountNotFoundException(accountNumber);

            if (account.Balance > 0)
            {
                throw new BankingDomainException($"Account {accountNumber} cannot be deleted because it has a remaining balance of ${account.Balance:F2}. Please withdraw or transfer funds first.");
            }

            _unitOfWork.Accounts.Delete(account);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync("System", "AccountDelete", "Account", accountNumber, $"Deleted account {accountNumber}.");
            return true;
        }

        public async Task<IEnumerable<AccountDto>> GetPendingAccountRequestsAsync()
        {
            var accounts = await _unitOfWork.Accounts.FindAsync(a => a.Status == AccountStatus.PendingApproval || a.Status == AccountStatus.PendingClosure);
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToDictionary(c => c.Id, c => c.FullName);
            return accounts.Select(a => MapToDto(a, customers.TryGetValue(a.CustomerId, out var name) ? name : "Customer"));
        }

        public async Task<AccountDto> ApproveAccountAsync(int accountId, bool approve, string adminUsername)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId) ?? throw new BankingDomainException($"Account ID {accountId} not found.");
            var customer = await _unitOfWork.Customers.GetByIdAsync(account.CustomerId);

            account.Status = approve ? AccountStatus.Active : AccountStatus.Rejected;
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync(adminUsername, approve ? "AccountApprove" : "AccountReject", "Account", account.AccountNumber, $"Admin {adminUsername} {(approve ? "approved" : "rejected")} account creation.");
            return MapToDto(account, customer?.FullName ?? "Customer");
        }

        public async Task<AccountDto> RequestAccountClosureAsync(string accountNumber, string requestedBy)
        {
            var list = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = list.FirstOrDefault() ?? throw new AccountNotFoundException(accountNumber);

            if (account.Balance > 0)
            {
                throw new BankingDomainException($"Account {accountNumber} cannot be submitted for closure because it has an active balance of ${account.Balance:F2}. Please withdraw or transfer funds first.");
            }

            account.Status = AccountStatus.PendingClosure;
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync(requestedBy, "AccountClosureRequest", "Account", accountNumber, $"Requested closure for account {accountNumber}. Awaiting Admin Approval.");
            var customer = await _unitOfWork.Customers.GetByIdAsync(account.CustomerId);
            return MapToDto(account, customer?.FullName ?? "Customer");
        }

        public async Task<AccountDto> ApproveAccountClosureAsync(int accountId, bool approve, string adminUsername)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId) ?? throw new BankingDomainException($"Account ID {accountId} not found.");
            var customer = await _unitOfWork.Customers.GetByIdAsync(account.CustomerId);

            if (approve)
            {
                account.Status = AccountStatus.Closed;
                await _unitOfWork.CompleteAsync();
                await _auditLogger.LogAsync(adminUsername, "AccountClosureApproved", "Account", account.AccountNumber, $"Admin {adminUsername} approved closure for account {account.AccountNumber}.");
            }
            else
            {
                account.Status = AccountStatus.Active;
                await _unitOfWork.CompleteAsync();
                await _auditLogger.LogAsync(adminUsername, "AccountClosureRejected", "Account", account.AccountNumber, $"Admin {adminUsername} rejected closure for account {account.AccountNumber}. Account reactivated.");
            }

            return MapToDto(account, customer?.FullName ?? "Customer");
        }

        private static AccountDto MapToDto(Account a, string customerName) => new AccountDto
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            CustomerId = a.CustomerId,
            CustomerName = customerName,
            AccountType = a.AccountType,
            Balance = a.Balance,
            Currency = a.Currency,
            Status = a.Status,
            InterestRate = a.InterestRate,
            OverdraftLimit = a.OverdraftLimit,
            CreatedAt = a.CreatedAt
        };
    }

    // AtmCardService Implementation
    public class AtmCardService : IAtmCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAtmCardFactory _cardFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditLogger _auditLogger;

        public AtmCardService(IUnitOfWork unitOfWork, IAtmCardFactory cardFactory, IPasswordHasher passwordHasher, IAuditLogger auditLogger)
        {
            _unitOfWork = unitOfWork;
            _cardFactory = cardFactory;
            _passwordHasher = passwordHasher;
            _auditLogger = auditLogger;
        }

        public async Task<AtmCardDto> RequestCardAsync(RequestAtmCardDto request, string performedBy = "User")
        {
            var accs = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == request.AccountNumber);
            var account = accs.FirstOrDefault() ?? throw new AccountNotFoundException(request.AccountNumber);

            var customer = await _unitOfWork.Customers.GetByIdAsync(account.CustomerId);
            string pinHash = _passwordHasher.HashPassword(request.InitialPin);

            var card = _cardFactory.CreateAtmCard(account, customer?.FullName ?? "VALUED CUSTOMER", request.CardType, pinHash);
            await _unitOfWork.AtmCards.AddAsync(card);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync(performedBy, "AtmCardIssue", "AtmCard", card.CardNumber, $"Issued new {card.CardType} card for account {account.AccountNumber}.");

            return MapToDto(card, account.AccountNumber);
        }

        public async Task<IEnumerable<AtmCardDto>> GetCustomerCardsAsync(int customerId)
        {
            var accounts = (await _unitOfWork.Accounts.FindAsync(a => a.CustomerId == customerId)).ToList();
            var accountIds = accounts.Select(a => a.Id).ToHashSet();
            var accMap = accounts.ToDictionary(a => a.Id, a => a.AccountNumber);

            var cards = await _unitOfWork.AtmCards.FindAsync(c => accountIds.Contains(c.AccountId));
            return cards.Select(c => MapToDto(c, accMap.GetValueOrDefault(c.AccountId, "Unknown")));
        }

        public async Task<IEnumerable<AtmCardDto>> GetAccountCardsAsync(string accountNumber)
        {
            var accs = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = accs.FirstOrDefault() ?? throw new AccountNotFoundException(accountNumber);

            var cards = await _unitOfWork.AtmCards.FindAsync(c => c.AccountId == account.Id);
            return cards.Select(c => MapToDto(c, accountNumber));
        }

        public async Task<IEnumerable<AtmCardDto>> GetAllCardsAsync()
        {
            var cards = await _unitOfWork.AtmCards.GetAllAsync();
            var accMap = (await _unitOfWork.Accounts.GetAllAsync()).ToDictionary(a => a.Id, a => a.AccountNumber);
            return cards.Select(c => MapToDto(c, accMap.GetValueOrDefault(c.AccountId, "Unknown")));
        }

        public async Task<IEnumerable<AtmCardDto>> GetPendingCardsAsync()
        {
            var cards = await _unitOfWork.AtmCards.FindAsync(c => c.Status == CardStatus.PendingApproval);
            var accMap = (await _unitOfWork.Accounts.GetAllAsync()).ToDictionary(a => a.Id, a => a.AccountNumber);
            return cards.Select(c => MapToDto(c, accMap.GetValueOrDefault(c.AccountId, "Unknown")));
        }

        public async Task<AtmCardDto> ApproveCardAsync(int cardId, bool approve, string adminUsername)
        {
            var card = await _unitOfWork.AtmCards.GetByIdAsync(cardId) ?? throw new BankingDomainException($"ATM Card ID {cardId} not found.");
            card.Status = approve ? CardStatus.Active : CardStatus.Rejected;
            await _unitOfWork.CompleteAsync();

            var acc = await _unitOfWork.Accounts.GetByIdAsync(card.AccountId);
            await _auditLogger.LogAsync(adminUsername, approve ? "AtmCardApprove" : "AtmCardReject", "AtmCard", card.CardNumber, $"Admin {adminUsername} {(approve ? "approved and issued" : "rejected")} ATM Card request.");

            return MapToDto(card, acc?.AccountNumber ?? "Unknown");
        }

        public async Task<bool> ChangePinAsync(int cardId, ChangePinDto pinDto)
        {
            var card = await _unitOfWork.AtmCards.GetByIdAsync(cardId);
            if (card == null) throw new BankingDomainException($"ATM Card with ID {cardId} not found.");

            if (!_passwordHasher.VerifyPassword(pinDto.OldPin, card.PinHash))
            {
                throw new BankingDomainException("Current PIN is incorrect.");
            }

            card.PinHash = _passwordHasher.HashPassword(pinDto.NewPin);
            _unitOfWork.AtmCards.Update(card);
            await _unitOfWork.CompleteAsync();

            await _auditLogger.LogAsync("User", "AtmCardPinChange", "AtmCard", card.CardNumber, "ATM Card PIN changed successfully.");
            return true;
        }

        public async Task<AtmCardDto> UpdateCardStatusAsync(int cardId, CardStatus status, string performedBy = "Admin")
        {
            var card = await _unitOfWork.AtmCards.GetByIdAsync(cardId);
            if (card == null) throw new BankingDomainException($"ATM Card with ID {cardId} not found.");

            card.Status = status;
            _unitOfWork.AtmCards.Update(card);
            await _unitOfWork.CompleteAsync();

            var acc = await _unitOfWork.Accounts.GetByIdAsync(card.AccountId);
            await _auditLogger.LogAsync(performedBy, "AtmCardStatusUpdate", "AtmCard", card.CardNumber, $"Updated card status to {status}.");

            return MapToDto(card, acc?.AccountNumber ?? "Unknown");
        }

        private static AtmCardDto MapToDto(AtmCard c, string accountNumber) => new AtmCardDto
        {
            Id = c.Id,
            AccountId = c.AccountId,
            AccountNumber = accountNumber,
            CardNumber = c.CardNumber,
            CardHolderName = c.CardHolderName,
            CardType = c.CardType,
            ExpiryDate = c.ExpiryDate,
            Cvv = c.Cvv,
            Status = c.Status,
            DailyWithdrawalLimit = c.DailyWithdrawalLimit,
            IssuedAt = c.IssuedAt
        };
    }

    // CoreTransactionService Implementation
    public class CoreTransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoreTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionDto> DepositAsync(DepositWithdrawDto dto, string performedBy = "User")
        {
            var strategy = new DepositTransactionStrategy();
            var tx = await strategy.ExecuteAsync(_unitOfWork, dto.AccountNumber, null, dto.Amount, dto.Description);
            await _unitOfWork.CompleteAsync();
            return MapToDto(tx, dto.AccountNumber, null);
        }

        public async Task<TransactionDto> WithdrawAsync(DepositWithdrawDto dto, string performedBy = "User")
        {
            var strategy = new WithdrawalTransactionStrategy();
            var tx = await strategy.ExecuteAsync(_unitOfWork, dto.AccountNumber, null, dto.Amount, dto.Description);
            await _unitOfWork.CompleteAsync();
            return MapToDto(tx, dto.AccountNumber, null);
        }

        public async Task<TransactionDto> TransferAsync(TransferDto dto, string performedBy = "User")
        {
            var strategy = new TransferTransactionStrategy();
            var tx = await strategy.ExecuteAsync(_unitOfWork, dto.SourceAccountNumber, dto.TargetAccountNumber, dto.Amount, dto.Description);
            await _unitOfWork.CompleteAsync();
            return MapToDto(tx, dto.SourceAccountNumber, dto.TargetAccountNumber);
        }

        public async Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(string accountNumber)
        {
            var accounts = await _unitOfWork.Accounts.FindAsync(a => a.AccountNumber == accountNumber);
            var account = accounts.FirstOrDefault() ?? throw new AccountNotFoundException(accountNumber);

            var txs = await _unitOfWork.Transactions.FindAsync(t => t.SourceAccountId == account.Id || t.TargetAccountId == account.Id);
            var allAccounts = (await _unitOfWork.Accounts.GetAllAsync()).ToDictionary(a => a.Id, a => a.AccountNumber);

            return txs.OrderByDescending(t => t.Timestamp).Select(t => MapToDto(t,
                allAccounts.GetValueOrDefault(t.SourceAccountId, "Unknown"),
                t.TargetAccountId.HasValue ? allAccounts.GetValueOrDefault(t.TargetAccountId.Value) : null));
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var txs = await _unitOfWork.Transactions.GetAllAsync();
            var allAccounts = (await _unitOfWork.Accounts.GetAllAsync()).ToDictionary(a => a.Id, a => a.AccountNumber);

            return txs.OrderByDescending(t => t.Timestamp).Select(t => MapToDto(t,
                allAccounts.GetValueOrDefault(t.SourceAccountId, "Unknown"),
                t.TargetAccountId.HasValue ? allAccounts.GetValueOrDefault(t.TargetAccountId.Value) : null));
        }

        private static TransactionDto MapToDto(Transaction t, string srcNum, string? tgtNum) => new TransactionDto
        {
            Id = t.Id,
            TransactionReference = t.TransactionReference,
            SourceAccountNumber = srcNum,
            TargetAccountNumber = tgtNum,
            Amount = t.Amount,
            FeeAmount = t.FeeAmount,
            TransactionType = t.TransactionType,
            Status = t.Status,
            Description = t.Description,
            Timestamp = t.Timestamp
        };
    }

    // LoanService Implementation
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoanApprovalStrategy _loanApprovalStrategy;
        private readonly ITransactionService _transactionService;

        public LoanService(IUnitOfWork unitOfWork, ILoanApprovalStrategy loanApprovalStrategy, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _loanApprovalStrategy = loanApprovalStrategy;
            _transactionService = transactionService;
        }

        public async Task<LoanDto> ApplyLoanAsync(LoanApplicationDto request)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new BankingDomainException($"Customer with ID {request.CustomerId} not found.");

            var (isApproved, reason, defaultRate) = _loanApprovalStrategy.EvaluateApplication(customer, request.LoanType, request.PrincipalAmount, request.TenureMonths);
            if (!isApproved)
            {
                throw new BankingDomainException($"Loan application rejected: {reason}");
            }

            var emiResult = CalculateEmi(new EmiCalculatorDto
            {
                PrincipalAmount = request.PrincipalAmount,
                AnnualInterestRate = defaultRate,
                TenureMonths = request.TenureMonths
            });

            var loanNumber = $"LN-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
            var loan = new Loan
            {
                LoanNumber = loanNumber,
                CustomerId = request.CustomerId,
                LoanType = request.LoanType,
                PrincipalAmount = request.PrincipalAmount,
                InterestRate = defaultRate,
                TenureMonths = request.TenureMonths,
                MonthlyEMI = emiResult.MonthlyEMI,
                RemainingAmount = emiResult.TotalPayment,
                Status = LoanStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.Loans.AddAsync(loan);
            await _unitOfWork.CompleteAsync();

            return MapToDto(loan, customer.FullName);
        }

        public async Task<LoanDto> ProcessLoanApprovalAsync(int loanId, LoanApprovalDto approvalDto)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null) throw new LoanNotFoundException(loanId);

            if (approvalDto.Approve)
            {
                loan.Status = LoanStatus.Active;
                loan.ApprovedAt = DateTime.UtcNow;
                loan.ApprovedBy = approvalDto.ActionBy;
            }
            else
            {
                loan.Status = LoanStatus.Rejected;
            }

            _unitOfWork.Loans.Update(loan);
            await _unitOfWork.CompleteAsync();

            var customer = await _unitOfWork.Customers.GetByIdAsync(loan.CustomerId);
            return MapToDto(loan, customer?.FullName ?? "Customer");
        }

        public async Task<TransactionDto> RepayEmiAsync(LoanRepaymentDto repaymentDto)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(repaymentDto.LoanId);
            if (loan == null) throw new LoanNotFoundException(repaymentDto.LoanId);

            if (loan.Status != LoanStatus.Active)
                throw new BankingDomainException($"Cannot repay loan in status: {loan.Status}");

            var tx = await _transactionService.WithdrawAsync(new DepositWithdrawDto
            {
                AccountNumber = repaymentDto.AccountNumber,
                Amount = repaymentDto.RepaymentAmount,
                Description = $"Loan Repayment for Loan {loan.LoanNumber}"
            });

            loan.RemainingAmount = Math.Max(0m, loan.RemainingAmount - repaymentDto.RepaymentAmount);
            if (loan.RemainingAmount == 0m)
            {
                loan.Status = LoanStatus.Closed;
            }

            _unitOfWork.Loans.Update(loan);
            await _unitOfWork.CompleteAsync();

            return tx;
        }

        public async Task<IEnumerable<LoanDto>> GetCustomerLoansAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            var loans = await _unitOfWork.Loans.FindAsync(l => l.CustomerId == customerId);
            return loans.Select(l => MapToDto(l, customer?.FullName ?? "Customer"));
        }

        public async Task<IEnumerable<LoanDto>> GetAllLoansAsync()
        {
            var loans = await _unitOfWork.Loans.GetAllAsync();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToDictionary(c => c.Id, c => c.FullName);
            return loans.Select(l => MapToDto(l, customers.GetValueOrDefault(l.CustomerId, "Customer")));
        }

        public EmiCalculatorResultDto CalculateEmi(EmiCalculatorDto dto)
        {
            if (dto.PrincipalAmount <= 0 || dto.AnnualInterestRate <= 0 || dto.TenureMonths <= 0)
            {
                return new EmiCalculatorResultDto();
            }

            double p = (double)dto.PrincipalAmount;
            double r = (double)dto.AnnualInterestRate / (12 * 100);
            int n = dto.TenureMonths;

            double emi = (p * r * Math.Pow(1 + r, n)) / (Math.Pow(1 + r, n) - 1);
            decimal monthlyEmi = Math.Round((decimal)emi, 2);
            decimal totalPayment = Math.Round(monthlyEmi * n, 2);
            decimal totalInterest = Math.Round(totalPayment - dto.PrincipalAmount, 2);

            return new EmiCalculatorResultDto
            {
                MonthlyEMI = monthlyEmi,
                TotalInterest = totalInterest,
                TotalPayment = totalPayment
            };
        }

        private static LoanDto MapToDto(Loan l, string customerName) => new LoanDto
        {
            Id = l.Id,
            LoanNumber = l.LoanNumber,
            CustomerId = l.CustomerId,
            CustomerName = customerName,
            LoanType = l.LoanType,
            PrincipalAmount = l.PrincipalAmount,
            InterestRate = l.InterestRate,
            TenureMonths = l.TenureMonths,
            MonthlyEMI = l.MonthlyEMI,
            RemainingAmount = l.RemainingAmount,
            Status = l.Status,
            AppliedAt = l.AppliedAt,
            ApprovedAt = l.ApprovedAt,
            ApprovedBy = l.ApprovedBy
        };
    }
}
