using GuestRegistration.DAL.Models;

namespace GuestRegistration.DAL.Interfaces;

public interface IUserRepository
{
    User Create(User user);
    List<User> GetAll();
    User? GetById(int id);
    User? GetByLogin(string login);
    User? GetByEmail(string email);
    bool Update(User user);
    bool Delete(int id);
}
