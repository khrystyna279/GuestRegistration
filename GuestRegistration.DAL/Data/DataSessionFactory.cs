using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GuestRegistration.DAL.Data;

public sealed class DataSessionFactory : IDataSessionFactory
{
    private readonly DbContextOptions<AppDbContext> _options;
    public DataSessionFactory(DbContextOptions<AppDbContext> options) => _options = options;
    public IDataSession Open(bool transactional = false) => new DataSession(_options, transactional);

    private sealed class DataSession : IDataSession
    {
        private readonly AppDbContext _context;
        private readonly IDbContextTransaction? _transaction;
        private bool _disposed;
        public IUserRepository Users { get; }
        public IBankCardRepository Cards { get; }
        public ILoginAttemptRepository Attempts { get; }

        public DataSession(DbContextOptions<AppDbContext> options, bool transactional)
        {
            _context = new AppDbContext(options);
            try
            {
                _transaction = transactional ? _context.Database.BeginTransaction() : null;
                Users = new UserRepository(_context);
                Cards = new BankCardRepository(_context);
                Attempts = new LoginAttemptRepository(_context);
            }
            catch { _context.Dispose(); throw; }
        }

        public void Commit() => _transaction?.Commit();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // An uncommitted transaction is rolled back on disposal.
            try { _transaction?.Dispose(); }
            finally { _context.Dispose(); }
        }
    }
}
