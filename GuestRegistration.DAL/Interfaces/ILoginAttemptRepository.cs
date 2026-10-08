using GuestRegistration.DAL.Models;

namespace GuestRegistration.DAL.Interfaces;

public interface ILoginAttemptRepository
{
    LoginAttempt Create(LoginAttempt attempt);
    List<LoginAttempt> GetAll();
    LoginAttempt? GetById(int id);
    bool Update(LoginAttempt attempt);
    bool Delete(int id);
}
