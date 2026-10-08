using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Repositories;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly AppDbContext _context;

    public LoginAttemptRepository(AppDbContext context) => _context = context;

    public LoginAttempt Create(LoginAttempt attempt)
    {
        _context.LoginAttempts.Add(attempt);
        _context.SaveChanges();
        return attempt;
    }

    public List<LoginAttempt> GetAll() => _context.LoginAttempts.AsNoTracking().OrderBy(attempt => attempt.Id).ToList();
    public LoginAttempt? GetById(int id) => _context.LoginAttempts.AsNoTracking().FirstOrDefault(attempt => attempt.Id == id);

    public bool Update(LoginAttempt attempt)
    {
        var existing = _context.LoginAttempts.Find(attempt.Id);
        if (existing == null) return false;
        _context.Entry(existing).CurrentValues.SetValues(attempt);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var attempt = _context.LoginAttempts.Find(id);
        if (attempt == null) return false;
        _context.LoginAttempts.Remove(attempt);
        _context.SaveChanges();
        return true;
    }
}
