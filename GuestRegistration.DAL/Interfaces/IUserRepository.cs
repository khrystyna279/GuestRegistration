using GuestRegistration.DTO;

namespace GuestRegistration.DAL.Interfaces;

public interface IUserRepository : IRepository<UserDto>
{
    UserDto? FindByLogin(string login);
    UserDto? FindByEmail(string email);
}
