using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using GuestRegistration.DTO;

namespace GuestRegistration.DAL.Repositories;

public sealed class LoginAttemptRepository : RepositoryBase<LoginAttempt, LoginAttemptDto>, ILoginAttemptRepository
{
    public LoginAttemptRepository(AppDbContext context) : base(context) { }

    protected override LoginAttemptDto ToDto(LoginAttempt entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        LoginEntered = entity.LoginEntered,
        AttemptedAtUtc = entity.AttemptedAtUtc,
        IsSuccessful = entity.IsSuccessful,
        RowInsertTime = entity.RowInsertTime,
        UpdateTime = entity.UpdateTime
    };

    protected override LoginAttempt ToEntity(LoginAttemptDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        LoginEntered = dto.LoginEntered,
        AttemptedAtUtc = dto.AttemptedAtUtc,
        IsSuccessful = dto.IsSuccessful
    };
}
