using GuestRegistration.DTO;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class BankCardRepositoryTests : RepositoryContractTests<BankCardDto>
{
    protected override IRepository<BankCardDto> Repository => new BankCardRepository(_database.Context);
    protected override BankCardDto NewRecord() => TestDatabase.NewCard(_database.AddUser().Id);
    protected override void ChangeRecord(BankCardDto dto) => dto.CardHolder = "CHANGED HOLDER";
    protected override string ReadMarker(BankCardDto dto) => dto.CardHolder;
    protected override string DirectUpdateSql => "UPDATE dbo.BankCards SET CardHolder = N'SQL changed', RowInsertTime = '20000101', UpdateTime = '20000101' WHERE Id = {0}";

    [Fact]
    public void Create_SavesCardWithUserForeignKey()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        Assert.True(card.Id > 0);
        Assert.Equal(user.Id, Repository.GetById(card.Id)!.UserId);
    }

    [Fact]
    public void GetAll_ReturnsCardsForDifferentUsers()
    {
        var first = _database.AddUser("first");
        var second = _database.AddUser("second");
        Repository.Create(TestDatabase.NewCard(first.Id));
        Repository.Create(TestDatabase.NewCard(first.Id));
        Repository.Create(TestDatabase.NewCard(second.Id));
        Assert.Equal(3, Repository.GetAll().Count);
        Assert.Equal(2, Repository.GetAll().Count(card => card.UserId == first.Id));
        Assert.Single(Repository.GetAll().Where(card => card.UserId == second.Id));
    }

    [Fact]
    public void GetById_ReturnsNullForMissingCard() => Assert.Null(Repository.GetById(999));

    [Fact]
    public void Update_ChangesPersistedCard()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        var changed = Repository.GetById(card.Id)!;
        changed.CardHolder = "CHANGED HOLDER";
        changed.ExpirationMonth = 6;
        Assert.True(Repository.Update(changed));
        Assert.Equal("CHANGED HOLDER", Repository.GetById(card.Id)!.CardHolder);
        Assert.Equal(6, Repository.GetById(card.Id)!.ExpirationMonth);
    }

    [Fact]
    public void Update_ReturnsFalseForMissingCard() => Assert.False(Repository.Update(new BankCardDto { Id = 999 }));

    [Fact]
    public void Delete_RemovesCard()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        Assert.True(Repository.Delete(card.Id));
        Assert.Null(Repository.GetById(card.Id));
        Assert.False(Repository.Delete(card.Id));
        Assert.NotNull(new UserRepository(_database.Context).GetById(user.Id));
    }

    [Fact]
    public void Create_RejectsMissingUser() =>
        Assert.Throws<DbUpdateException>(() => Repository.Create(TestDatabase.NewCard(999)));

    [Fact]
    public void Update_RejectsMissingUser()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        var changed = Repository.GetById(card.Id)!;
        changed.UserId = 999;
        Assert.Throws<DbUpdateException>(() => Repository.Update(changed));
        Assert.Equal(user.Id, Repository.GetById(card.Id)!.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_RejectsInvalidExpirationMonth(int month)
    {
        var user = _database.AddUser();
        var card = TestDatabase.NewCard(user.Id);
        card.ExpirationMonth = month;
        Assert.Throws<DbUpdateException>(() => Repository.Create(card));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcd")]
    public void Create_RejectsInvalidLastFourDigits(string digits)
    {
        var user = _database.AddUser();
        var card = TestDatabase.NewCard(user.Id);
        card.LastFourDigits = digits;
        Assert.Throws<DbUpdateException>(() => Repository.Create(card));
    }

    [Fact]
    public void Transaction_RollbackCancelsRepositorySave()
    {
        var user = _database.AddUser();
        using (var transaction = _database.Context.Database.BeginTransaction())
        {
            Repository.Create(TestDatabase.NewCard(user.Id));
            transaction.Rollback();
        }
        _database.Context.ChangeTracker.Clear();
        Assert.Empty(Repository.GetAll());
    }

}
