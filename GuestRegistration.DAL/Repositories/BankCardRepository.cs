using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Repositories;

public class BankCardRepository : IBankCardRepository
{
    private readonly AppDbContext _context;

    public BankCardRepository(AppDbContext context) => _context = context;

    public BankCard Create(BankCard card)
    {
        _context.BankCards.Add(card);
        _context.SaveChanges();
        return card;
    }

    public List<BankCard> GetAll() => _context.BankCards.AsNoTracking().OrderBy(card => card.Id).ToList();
    public BankCard? GetById(int id) => _context.BankCards.AsNoTracking().FirstOrDefault(card => card.Id == id);
    public List<BankCard> GetByUserId(int userId) => _context.BankCards.AsNoTracking()
        .Where(card => card.UserId == userId).OrderBy(card => card.Id).ToList();

    public bool Update(BankCard card)
    {
        var existing = _context.BankCards.Find(card.Id);
        if (existing == null) return false;
        _context.Entry(existing).CurrentValues.SetValues(card);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var card = _context.BankCards.Find(id);
        if (card == null) return false;
        _context.BankCards.Remove(card);
        _context.SaveChanges();
        return true;
    }
}
