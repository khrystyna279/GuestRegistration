using GuestRegistration.DAL.Models;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class BankCardRepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private BankCardRepository Repository => new(_database.Context);

    [SqlServerFact]
    public void Create_SavesCardWithUserForeignKey()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        Assert.True(card.Id > 0);
        Assert.Equal(user.Id, Repository.GetById(card.Id)!.UserId);
    }

    [SqlServerFact]
    public void GetAllAndGetByUserId_ReturnCorrectCards()
    {
        var first = _database.AddUser("first");
        var second = _database.AddUser("second");
        Repository.Create(TestDatabase.NewCard(first.Id));
        Repository.Create(TestDatabase.NewCard(first.Id));
        Repository.Create(TestDatabase.NewCard(second.Id));
        Assert.Equal(3, Repository.GetAll().Count);
        Assert.Equal(2, Repository.GetByUserId(first.Id).Count);
        Assert.Empty(Repository.GetByUserId(999));
    }

    [SqlServerFact]
    public void GetById_ReturnsNullForMissingCard() => Assert.Null(Repository.GetById(999));

    [SqlServerFact]
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

    [SqlServerFact]
    public void Update_ReturnsFalseForMissingCard() => Assert.False(Repository.Update(new BankCard { Id = 999 }));

    [SqlServerFact]
    public void Delete_RemovesCard()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        Assert.True(Repository.Delete(card.Id));
        Assert.Null(Repository.GetById(card.Id));
        Assert.False(Repository.Delete(card.Id));
        Assert.NotNull(new UserRepository(_database.Context).GetById(user.Id));
    }

    [SqlServerFact]
    public void Create_RejectsMissingUser() =>
        Assert.Throws<DbUpdateException>(() => Repository.Create(TestDatabase.NewCard(999)));

    [SqlServerFact]
    public void Update_RejectsMissingUser()
    {
        var user = _database.AddUser();
        var card = Repository.Create(TestDatabase.NewCard(user.Id));
        var changed = Repository.GetById(card.Id)!;
        changed.UserId = 999;
        Assert.Throws<DbUpdateException>(() => Repository.Update(changed));
        Assert.Equal(user.Id, Repository.GetById(card.Id)!.UserId);
    }

    [SqlServerTheory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_RejectsInvalidExpirationMonth(int month)
    {
        var user = _database.AddUser();
        var card = TestDatabase.NewCard(user.Id);
        card.ExpirationMonth = month;
        Assert.Throws<DbUpdateException>(() => Repository.Create(card));
    }

    [SqlServerTheory]
    [InlineData("123")]
    [InlineData("abcd")]
    public void Create_RejectsInvalidLastFourDigits(string digits)
    {
        var user = _database.AddUser();
        var card = TestDatabase.NewCard(user.Id);
        card.LastFourDigits = digits;
        Assert.Throws<DbUpdateException>(() => Repository.Create(card));
    }

    [SqlServerFact]
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

    public void Dispose() => _database.Dispose();
}
