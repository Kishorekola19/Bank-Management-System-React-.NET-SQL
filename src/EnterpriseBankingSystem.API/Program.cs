using System.Text;
using EnterpriseBankingSystem.API.Middleware;
using EnterpriseBankingSystem.Application.Decorators;
using EnterpriseBankingSystem.Application.Factories;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Application.Services;
using EnterpriseBankingSystem.Application.Strategies;
using EnterpriseBankingSystem.Infrastructure.Data;
using EnterpriseBankingSystem.Infrastructure.Repositories;
using EnterpriseBankingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers and configure JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=banking.db";
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Application & Infrastructure Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

builder.Services.AddScoped<IAccountFactory, AccountFactory>();
builder.Services.AddScoped<IAtmCardFactory, AtmCardFactory>();
builder.Services.AddScoped<ILoanApprovalStrategy, DefaultLoanApprovalStrategy>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAtmCardService, AtmCardService>();
builder.Services.AddScoped<ILoanService, LoanService>();

// Transaction Service with Audit Decorator
builder.Services.AddScoped<CoreTransactionService>();
builder.Services.AddScoped<ITransactionService>(provider =>
{
    var coreService = provider.GetRequiredService<CoreTransactionService>();
    var auditLogger = provider.GetRequiredService<IAuditLogger>();
    return new LoggingTransactionDecorator(coreService, auditLogger);
});

// Configure JWT Authentication
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "EnterpriseBankingSystem_Super_Secret_JWT_Key_2026_For_Security_123456789";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "EnterpriseBankingSystemAPI";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "EnterpriseBankingSystemClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Configure CORS for React Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise Banking Management System API",
        Version = "v1",
        Description = "ASP.NET Core REST API for Enterprise Banking System"
    });
});

var app = builder.Build();

// Ensure Database Schema is Created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure HTTP Middleware Pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise Banking API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowReactApp");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
