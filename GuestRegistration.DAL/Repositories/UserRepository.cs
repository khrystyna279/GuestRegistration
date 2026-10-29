using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using GuestRegistration.DTO;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Repositories;

public sealed class UserRepository : RepositoryBase<User, UserDto>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public UserDto? FindByLogin(string login)
    {
        var user = Context.Users.AsNoTracking().FirstOrDefault(user => user.Login == login);
        return user == null ? null : ToDto(user);
    }

    public UserDto? FindByEmail(string email)
    {
        var user = Context.Users.AsNoTracking().FirstOrDefault(user => user.Email == email);
        return user == null ? null : ToDto(user);
    }

    protected override UserDto ToDto(User entity) => new()
    {
        Id = entity.Id,
        RoleCode = entity.RoleCode,
        Login = entity.Login,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        PasswordHash = entity.PasswordHash,
        RecoveryWordHash = entity.RecoveryWordHash,
        Gender = entity.Gender,
        Address = entity.Address,
        Email = entity.Email,
        Phone = entity.Phone,
        CreatedAtUtc = entity.CreatedAtUtc,
        RowInsertTime = entity.RowInsertTime,
        UpdateTime = entity.UpdateTime
    };

    protected override User ToEntity(UserDto dto) => new()
    {
        Id = dto.Id,
        RoleCode = dto.RoleCode,
        Login = dto.Login,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        PasswordHash = dto.PasswordHash,
        RecoveryWordHash = dto.RecoveryWordHash,
        Gender = dto.Gender,
        Address = dto.Address,
        Email = dto.Email,
        Phone = dto.Phone,
        CreatedAtUtc = dto.CreatedAtUtc
    };
}
