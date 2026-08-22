using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Application.Services;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseBankingSystem.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return string.Equals(hash, passwordHash, StringComparison.Ordinal);
        }
    }

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            var secretKey = _configuration["JwtSettings:Secret"] ?? "EnterpriseBankingSystem_Super_Secret_JWT_Key_2026_For_Security_123456789";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "EnterpriseBankingSystemAPI";
            var audience = _configuration["JwtSettings:Audience"] ?? "EnterpriseBankingSystemClients";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("CustomerId", user.CustomerId?.ToString() ?? "")
            };

            var expiresAt = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }

    public class AuditLogger : IAuditLogger
    {
        private readonly BankingDbContext _context;

        public AuditLogger(BankingDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string username, string action, string entityName, string entityId, string details, string ipAddress = "127.0.0.1")
        {
            var log = new AuditLog
            {
                Username = username,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
