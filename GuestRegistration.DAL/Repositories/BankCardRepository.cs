using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using GuestRegistration.DTO;

namespace GuestRegistration.DAL.Repositories;

public sealed class BankCardRepository : RepositoryBase<BankCard, BankCardDto>, IBankCardRepository
{
    public BankCardRepository(AppDbContext context) : base(context) { }

    protected override BankCardDto ToDto(BankCard entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        CardHolder = entity.CardHolder,
        Brand = entity.Brand,
        LastFourDigits = entity.LastFourDigits,
        ExpirationMonth = entity.ExpirationMonth,
        ExpirationYear = entity.ExpirationYear,
        RowInsertTime = entity.RowInsertTime,
        UpdateTime = entity.UpdateTime
    };

    protected override BankCard ToEntity(BankCardDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        CardHolder = dto.CardHolder,
        Brand = dto.Brand,
        LastFourDigits = dto.LastFourDigits,
        ExpirationMonth = dto.ExpirationMonth,
        ExpirationYear = dto.ExpirationYear
    };
}
