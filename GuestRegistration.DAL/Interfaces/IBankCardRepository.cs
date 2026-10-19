using GuestRegistration.DAL.Models;

namespace GuestRegistration.DAL.Interfaces;

public interface IBankCardRepository
{
    BankCard Create(BankCard card);
    List<BankCard> GetAll();
    BankCard? GetById(int id);
    bool Update(BankCard card);
    bool Delete(int id);
}
