using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.DTOs;
using EnterpriseBankingSystem.Application.Services;
using EnterpriseBankingSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseBankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AtmCardController : ControllerBase
    {
        private readonly IAtmCardService _cardService;

        public AtmCardController(IAtmCardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost("request")]
        public async Task<ActionResult<AtmCardDto>> RequestCard([FromBody] RequestAtmCardDto request)
        {
            var performedBy = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var card = await _cardService.RequestCardAsync(request, performedBy);
            return Ok(card);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<AtmCardDto>>> GetCustomerCards(int customerId)
        {
            var cards = await _cardService.GetCustomerCardsAsync(customerId);
            return Ok(cards);
        }

        [HttpGet("account/{accountNumber}")]
        public async Task<ActionResult<IEnumerable<AtmCardDto>>> GetAccountCards(string accountNumber)
        {
            var cards = await _cardService.GetAccountCardsAsync(accountNumber);
            return Ok(cards);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Teller")]
        public async Task<ActionResult<IEnumerable<AtmCardDto>>> GetAllCards()
        {
            var cards = await _cardService.GetAllCardsAsync();
            return Ok(cards);
        }

        [HttpPost("{cardId:int}/pin")]
        public async Task<IActionResult> ChangePin(int cardId, [FromBody] ChangePinDto pinDto)
        {
            await _cardService.ChangePinAsync(cardId, pinDto);
            return Ok(new { message = "ATM Card PIN changed successfully." });
        }

        [HttpPost("{cardId:int}/status")]
        public async Task<ActionResult<AtmCardDto>> UpdateStatus(int cardId, [FromBody] UpdateCardStatusDto dto)
        {
            var performedBy = User.FindFirstValue(ClaimTypes.Email) ?? "User";
            var card = await _cardService.UpdateCardStatusAsync(cardId, dto.Status, performedBy);
            return Ok(card);
        }
    }
}
