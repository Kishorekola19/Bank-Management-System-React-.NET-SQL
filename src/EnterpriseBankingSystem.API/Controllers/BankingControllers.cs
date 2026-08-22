using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.DTOs;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Application.Services;
using EnterpriseBankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseBankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var customerId = User.FindFirstValue("CustomerId");
            var name = User.FindFirstValue(ClaimTypes.Name);

            return Ok(new
            {
                Email = email,
                Role = role,
                CustomerId = customerId,
                FullName = name
            });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound($"Customer with ID {id} not found.");
            return Ok(customer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] RegisterRequestDto request)
        {
            var customer = await _customerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto request)
        {
            var account = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetByAccountNumber), new { accountNumber = account.AccountNumber }, account);
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<AccountDto>> GetByAccountNumber(string accountNumber)
        {
            var account = await _accountService.GetByAccountNumberAsync(accountNumber);
            if (account == null) return NotFound($"Account {accountNumber} not found.");
            return Ok(account);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetCustomerAccounts(int customerId)
        {
            var accounts = await _accountService.GetCustomerAccountsAsync(customerId);
            return Ok(accounts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("{accountNumber}/interest")]
        public async Task<ActionResult<object>> CalculateInterest(string accountNumber)
        {
            var interest = await _accountService.CalculateAnnualInterestAsync(accountNumber);
            return Ok(new { AccountNumber = accountNumber, ProjectedAnnualInterest = interest });
        }

        [HttpPost("{accountNumber}/request-closure")]
        public async Task<IActionResult> RequestAccountClosure(string accountNumber)
        {
            var username = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var result = await _accountService.RequestAccountClosureAsync(accountNumber, username);
            return Ok(result);
        }

        [HttpDelete("{accountNumber}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(string accountNumber)
        {
            await _accountService.DeleteAccountAsync(accountNumber);
            return Ok(new { message = $"Account {accountNumber} closed and deleted successfully." });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult<TransactionDto>> Deposit([FromBody] DepositWithdrawDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var result = await _transactionService.DepositAsync(dto, username);
            return Ok(result);
        }

        [HttpPost("withdraw")]
        public async Task<ActionResult<TransactionDto>> Withdraw([FromBody] DepositWithdrawDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var result = await _transactionService.WithdrawAsync(dto, username);
            return Ok(result);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<TransactionDto>> Transfer([FromBody] TransferDto dto)
        {
            var username = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var result = await _transactionService.TransferAsync(dto, username);
            return Ok(result);
        }

        [HttpGet("account/{accountNumber}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(string accountNumber)
        {
            var history = await _transactionService.GetAccountTransactionsAsync(accountNumber);
            return Ok(history);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            var txs = await _transactionService.GetAllTransactionsAsync();
            return Ok(txs);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost("apply")]
        public async Task<ActionResult<LoanDto>> ApplyLoan([FromBody] LoanApplicationDto request)
        {
            var loan = await _loanService.ApplyLoanAsync(request);
            return Ok(loan);
        }

        [HttpPost("{loanId:int}/process")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<LoanDto>> ProcessLoan(int loanId, [FromBody] LoanApprovalDto approvalDto)
        {
            approvalDto.ActionBy = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var loan = await _loanService.ProcessLoanApprovalAsync(loanId, approvalDto);
            return Ok(loan);
        }

        [HttpPost("repay")]
        public async Task<ActionResult<TransactionDto>> RepayEmi([FromBody] LoanRepaymentDto repaymentDto)
        {
            var result = await _loanService.RepayEmiAsync(repaymentDto);
            return Ok(result);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetCustomerLoans(int customerId)
        {
            var loans = await _loanService.GetCustomerLoansAsync(customerId);
            return Ok(loans);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        [HttpPost("calculate-emi")]
        [AllowAnonymous]
        public ActionResult<EmiCalculatorResultDto> CalculateEmi([FromBody] EmiCalculatorDto dto)
        {
            var result = _loanService.CalculateEmi(dto);
            return Ok(result);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();
            return Ok(logs);
        }
    }
}
