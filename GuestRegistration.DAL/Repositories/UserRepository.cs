using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public User Create(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public List<User> GetAll() => _context.Users.AsNoTracking().OrderBy(user => user.Id).ToList();
    public User? GetById(int id) => _context.Users.AsNoTracking().FirstOrDefault(user => user.Id == id);
    public User? GetByLogin(string login)
    {
        string normalizedLogin = login.Trim().ToLowerInvariant();
        return _context.Users.AsNoTracking().FirstOrDefault(user => user.Login == normalizedLogin);
    }
    public User? GetByEmail(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        return _context.Users.AsNoTracking().FirstOrDefault(user => user.Email == normalizedEmail);
    }

    public bool Update(User user)
    {
        var existing = _context.Users.Find(user.Id);
        if (existing == null) return false;
        _context.Entry(existing).CurrentValues.SetValues(user);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        _context.SaveChanges();
        return true;
    }
}
