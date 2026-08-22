# Apex Enterprise Banking Management System

A full-stack enterprise digital banking and financial management solution engineered with ASP.NET Core Web API, React (Vite & Tailwind CSS), and Entity Framework Core with SQL/SQLite database integration.

[![Live Cloud Application](https://img.shields.io/badge/Live%20Application-Apex%20Banking%20Portal-0052CC?style=for-the-badge&logo=render)](https://apex-enterprise-banking-management-system.onrender.com/)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github)](https://github.com/Kishorekola19/Bank-Management-System-React-.NET-SQL)

---

## 🌐 Live Application Links & Testing

Experience the live full-stack application online:
👉 **[Launch Full-Stack Cloud Application](https://apex-enterprise-banking-management-system.onrender.com/)**

### Pre-Configured Demo Accounts for Validation

| Role | Username / Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Customer** | `john.doe@bank.com` | `User@123` | Accounts, Fund Transfers, ATM Cards, Loan Applications |
| **Administrator** | `Kishore` | `Kishore19@` | Approval Workflows, Customer Management, Audit Logs |

---

## Technical Highlights & Architecture

### Backend Architectural Design (.NET Web API)
- **Clean Architecture & Layered Separation**:
  - `EnterpriseBankingSystem.API`: RESTful Controllers, JWT Authentication, and Global Exception Middleware.
  - `EnterpriseBankingSystem.Application`: Core Business Services, DTOs, Factory Classes, and Strategy Implementations.
  - `EnterpriseBankingSystem.Domain`: Domain Entities, Business Exceptions, and Enums.
  - `EnterpriseBankingSystem.Infrastructure`: Entity Framework Core DbContext, Repositories, Unit of Work, and Service Adapters.
- **Enterprise Design Patterns**:
  - **Repository & Unit of Work Pattern**: Centralized data access abstraction for atomic transaction management.
  - **Factory Pattern**: Encapsulated creation of banking accounts and ATM card instances (`AccountFactory`, `AtmCardFactory`).
  - **Strategy Pattern**: Flexible interest calculation (`SavingsInterestStrategy`, `CheckingInterestStrategy`, `FixedDepositInterestStrategy`) and loan risk scoring.
  - **Decorator Pattern**: Cross-cutting audit logging wrapper around core transaction handling (`LoggingTransactionDecorator`).
- **Security & Authorization**:
  - Stateless JSON Web Token (JWT) authentication.
  - Role-based authorization controls (`Admin`, `Customer`, `Teller`).
  - Comprehensive audit trail recording user operations and administrative actions.

### Frontend Architectural Design (React SPA)
- **Framework**: React 19 powered by Vite for fast development and optimized bundle delivery.
- **Styling**: Tailwind CSS v4 featuring responsive design, dark/light contrast elements, and custom interactive UI components.
- **State & Communication**: Context API for global authentication state management (`AuthContext`) and Axios interceptors for automated JWT injection.

---

## Core Features

1. **User Authentication & Role Control**:
   - Secure registration and sign-in for Customers and Administrators.
   - Session retention via local secure tokens with automated expiry verification.

2. **Account Management**:
   - Create Savings, Checking, and Fixed Deposit accounts.
   - Real-time balance tracking, interest projection, and account closure workflows.

3. **Fund Transfers & Transactions**:
   - Instant deposits, withdrawals, and peer-to-peer account transfers.
   - Transaction reference generation and audit logging.

4. **ATM Card Lifecycle**:
   - Request Visa Debit, Mastercard Debit, and RuPay cards.
   - Manage card status (Active, Blocked, Frozen) and PIN changes.

5. **Loan Management & EMI Calculator**:
   - Apply for Personal, Home, and Vehicle loans with automatic EMI computation.
   - Interactive EMI estimation utility for principal, tenure, and interest rates.

6. **Admin Approval Center & Audit Logs**:
   - Review pending account openings, closure requests, card issuances, and loan applications.
   - Centralized system audit logs for administrative oversight.

---

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (or .NET 8.0+)
- [Node.js](https://nodejs.org/) (v18+ recommended) & npm

### Running the Application

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Kishorekola19/Bank-Management-System-React-.NET-SQL.git
   cd Bank-Management-System-React-.NET-SQL
   ```

2. **Build and Run the Solution**:
   ```bash
   dotnet build EnterpriseBankingSystem.sln
   dotnet run --project src/EnterpriseBankingSystem.API
   ```
   The API server will launch at `http://localhost:5000` (or `https://localhost:5001`), hosting both Swagger API Documentation (`/swagger`) and serving the integrated React web frontend.

3. **Running Web Frontend in Development Mode (Optional)**:
   ```bash
   cd src/EnterpriseBankingSystem.Web
   npm install
   npm run dev
   ```
   Access the frontend dev portal at `http://localhost:5173`.

---

## Project Structure

```
EnterpriseBankingSystem/
├── EnterpriseBankingSystem.sln
├── README.md
├── src/
│   ├── EnterpriseBankingSystem.API/           # Web API Controllers & Config
│   ├── EnterpriseBankingSystem.Application/   # DTOs, Services, Factories, Strategies
│   ├── EnterpriseBankingSystem.Domain/        # Domain Models & Enums
│   ├── EnterpriseBankingSystem.Infrastructure/ # EF Core DbContext & Repositories
│   └── EnterpriseBankingSystem.Web/           # React + Vite Frontend App
```
