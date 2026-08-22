using System;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Domain.Enums;
using EnterpriseBankingSystem.Domain.Exceptions;

namespace EnterpriseBankingSystem.Application.Factories
{
    public interface IAtmCardFactory
    {
        AtmCard CreateAtmCard(Account account, string holderName, CardType cardType, string pinHash);
    }

    public class AtmCardFactory : IAtmCardFactory
    {
        public AtmCard CreateAtmCard(Account account, string holderName, CardType cardType, string pinHash)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (account.Status != AccountStatus.Active)
                throw new BankingDomainException($"Cannot issue ATM Card for inactive account {account.AccountNumber}");

            string cardNumber = GenerateCardNumber(cardType);
            string cvv = new Random().Next(100, 999).ToString();
            string expiryDate = DateTime.UtcNow.AddYears(3).ToString("MM/yy");

            decimal dailyLimit = cardType switch
            {
                CardType.VisaDebit => 2500.00m,
                CardType.MastercardDebit => 3000.00m,
                CardType.PlatinumRuPay => 5000.00m,
                _ => 2000.00m
            };

            return new AtmCard
            {
                AccountId = account.Id,
                CardNumber = cardNumber,
                CardHolderName = string.IsNullOrWhiteSpace(holderName) ? "VALUED CUSTOMER" : holderName.ToUpper(),
                CardType = cardType,
                ExpiryDate = expiryDate,
                Cvv = cvv,
                PinHash = pinHash,
                Status = CardStatus.PendingApproval,
                DailyWithdrawalLimit = dailyLimit,
                IssuedAt = DateTime.UtcNow
            };
        }

        private static string GenerateCardNumber(CardType cardType)
        {
            string prefix = cardType switch
            {
                CardType.VisaDebit => "4532",
                CardType.MastercardDebit => "5412",
                CardType.PlatinumRuPay => "6011",
                _ => "4000"
            };

            var random = new Random();
            long r1 = random.NextInt64(1000, 9999);
            long r2 = random.NextInt64(1000, 9999);
            long r3 = random.NextInt64(1000, 9999);

            return $"{prefix} {r1} {r2} {r3}";
        }
    }
}
