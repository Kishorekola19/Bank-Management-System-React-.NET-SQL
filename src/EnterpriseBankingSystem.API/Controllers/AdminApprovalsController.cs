using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.DTOs;
using EnterpriseBankingSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseBankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminApprovalsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAtmCardService _cardService;
        private readonly ILoanService _loanService;

        public AdminApprovalsController(IAccountService accountService, IAtmCardService cardService, ILoanService loanService)
        {
            _accountService = accountService;
            _cardService = cardService;
            _loanService = loanService;
        }

        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var pendingAccounts = await _accountService.GetPendingAccountRequestsAsync();
            var pendingCards = await _cardService.GetPendingCardsAsync();
            var pendingLoans = await _loanService.GetAllLoansAsync();

            return Ok(new
            {
                PendingAccountOpenings = pendingAccounts.Where(a => a.Status == Domain.Enums.AccountStatus.PendingApproval),
                PendingAccountClosures = pendingAccounts.Where(a => a.Status == Domain.Enums.AccountStatus.PendingClosure),
                PendingCards = pendingCards,
                PendingLoans = pendingLoans.Where(l => l.Status == Domain.Enums.LoanStatus.Pending)
            });
        }

        [HttpPost("approve-account/{accountId:int}")]
        public async Task<IActionResult> ApproveAccount(int accountId, [FromQuery] bool approve = true)
        {
            var adminUsername = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Admin";
            var result = await _accountService.ApproveAccountAsync(accountId, approve, adminUsername);
            return Ok(result);
        }

        [HttpPost("approve-account-closure/{accountId:int}")]
        public async Task<IActionResult> ApproveAccountClosure(int accountId, [FromQuery] bool approve = true)
        {
            var adminUsername = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Admin";
            var result = await _accountService.ApproveAccountClosureAsync(accountId, approve, adminUsername);
            return Ok(result);
        }

        [HttpPost("approve-card/{cardId:int}")]
        public async Task<IActionResult> ApproveCard(int cardId, [FromQuery] bool approve = true)
        {
            var adminUsername = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Admin";
            var result = await _cardService.ApproveCardAsync(cardId, approve, adminUsername);
            return Ok(result);
        }

        [HttpPost("approve-loan/{loanId:int}")]
        public async Task<IActionResult> ApproveLoan(int loanId, [FromQuery] bool approve = true)
        {
            var adminUsername = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Admin";
            var result = await _loanService.ProcessLoanApprovalAsync(loanId, new LoanApprovalDto { Approve = approve, ActionBy = adminUsername });
            return Ok(result);
        }
    }
}
