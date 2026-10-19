using GuestRegistration.DAL.Models;

namespace GuestRegistration.DAL.Interfaces;

public interface IUserRepository
{
    User Create(User user);
    List<User> GetAll();
    User? GetById(int id);
    bool Update(User user);
    bool Delete(int id);
}
