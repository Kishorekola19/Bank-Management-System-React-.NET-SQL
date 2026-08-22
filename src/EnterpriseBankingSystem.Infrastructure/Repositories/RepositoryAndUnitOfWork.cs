using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EnterpriseBankingSystem.Application.Interfaces;
using EnterpriseBankingSystem.Domain.Entities;
using EnterpriseBankingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseBankingSystem.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BankingDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(BankingDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankingDbContext _context;

        public IRepository<User> Users { get; }
        public IRepository<Customer> Customers { get; }
        public IRepository<Account> Accounts { get; }
        public IRepository<AtmCard> AtmCards { get; }
        public IRepository<Transaction> Transactions { get; }
        public IRepository<Loan> Loans { get; }
        public IRepository<AuditLog> AuditLogs { get; }

        public UnitOfWork(BankingDbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            Customers = new Repository<Customer>(_context);
            Accounts = new Repository<Account>(_context);
            AtmCards = new Repository<AtmCard>(_context);
            Transactions = new Repository<Transaction>(_context);
            Loans = new Repository<Loan>(_context);
            AuditLogs = new Repository<AuditLog>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
