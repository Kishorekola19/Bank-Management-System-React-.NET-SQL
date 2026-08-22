using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Domain.Entities;

namespace EnterpriseBankingSystem.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }

    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Account> Accounts { get; }
        IRepository<AtmCard> AtmCards { get; }
        IRepository<Transaction> Transactions { get; }
        IRepository<Loan> Loans { get; }
        IRepository<AuditLog> AuditLogs { get; }
        Task<int> CompleteAsync();
    }
}
