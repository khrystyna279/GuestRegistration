using GuestRegistration.DTO;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class UserRepositoryTests : RepositoryContractTests<UserDto>
{
    protected override IRepository<UserDto> Repository => new UserRepository(_database.Context);
    protected override UserDto NewRecord() => TestDatabase.NewUser();
    protected override void ChangeRecord(UserDto dto) => dto.FirstName = "Changed";
    protected override string ReadMarker(UserDto dto) => dto.FirstName;
    protected override string DirectUpdateSql => "UPDATE dbo.Users SET FirstName = N'SQL changed', RowInsertTime = '20000101', UpdateTime = '20000101' WHERE Id = {0}";

    [Fact]
    public void Create_SavesUserAndGeneratesId()
    {
        var user = Repository.Create(TestDatabase.NewUser());
        Assert.True(user.Id > 0);
        Assert.Equal("test_user", Repository.GetById(user.Id)!.Login);
    }

    [Fact]
    public void GetAll_ReturnsSavedUsersInIdOrder()
    {
        Repository.Create(TestDatabase.NewUser("first"));
        Repository.Create(TestDatabase.NewUser("second"));
        Assert.Equal(new[] { "first", "second" }, Repository.GetAll().Select(user => user.Login));
    }

    [Fact]
    public void GetById_ReturnsNullForMissingUser() => Assert.Null(Repository.GetById(999));

    [Fact]
    public void FindByLoginAndEmail_ReturnsDtoOrNull()
    {
        var repository = new UserRepository(_database.Context);
        var user = repository.Create(TestDatabase.NewUser());
        Assert.Equal(user.Id, repository.FindByLogin(user.Login)!.Id);
        Assert.Equal(user.Id, repository.FindByEmail(user.Email)!.Id);
        Assert.Null(repository.FindByLogin("missing"));
        Assert.Null(repository.FindByEmail("missing@example.com"));
    }

    [Fact]
    public void Update_ChangesPersistedFields()
    {
        var user = Repository.Create(TestDatabase.NewUser());
        var changed = Repository.GetById(user.Id)!;
        changed.FirstName = "Changed";
        changed.Phone = "+380999999999";
        Assert.True(Repository.Update(changed));
        Assert.Equal("Changed", Repository.GetById(user.Id)!.FirstName);
        Assert.Equal("+380999999999", Repository.GetById(user.Id)!.Phone);
    }

    [Fact]
    public void Update_ReturnsFalseForMissingUser() => Assert.False(Repository.Update(new UserDto { Id = 999 }));

    [Fact]
    public void Delete_RemovesUser()
    {
        var user = Repository.Create(TestDatabase.NewUser());
        Assert.True(Repository.Delete(user.Id));
        Assert.Null(Repository.GetById(user.Id));
        Assert.False(Repository.Delete(user.Id));
    }

    [Fact]
    public void Create_RejectsDuplicateLogin()
    {
        Repository.Create(TestDatabase.NewUser());
        var duplicate = TestDatabase.NewUser();
        duplicate.Email = "different@example.com";
        Assert.Throws<DbUpdateException>(() => Repository.Create(duplicate));
        Assert.Single(Repository.GetAll());
    }

    [Fact]
    public void Create_RejectsDuplicateEmail()
    {
        Repository.Create(TestDatabase.NewUser());
        var duplicate = TestDatabase.NewUser("different");
        duplicate.Email = "test_user@example.com";
        Assert.Throws<DbUpdateException>(() => Repository.Create(duplicate));
    }

    [Fact]
    public void Update_RejectsDuplicateLogin()
    {
        Repository.Create(TestDatabase.NewUser("first"));
        var second = Repository.Create(TestDatabase.NewUser("second"));
        var changed = Repository.GetById(second.Id)!;
        changed.Login = "first";
        Assert.Throws<DbUpdateException>(() => Repository.Update(changed));
        Assert.Equal("second", Repository.GetById(second.Id)!.Login);
    }

    [Fact]
    public void Delete_CascadesCardsAndKeepsHistoryWithNullUserId()
    {
        var user = Repository.Create(TestDatabase.NewUser());
        var cards = new BankCardRepository(_database.Context);
        var attempts = new LoginAttemptRepository(_database.Context);
        cards.Create(TestDatabase.NewCard(user.Id));
        var attempt = attempts.Create(TestDatabase.NewAttempt(user.Id));
        _database.Context.ChangeTracker.Clear();
        Repository.Delete(user.Id);
        Assert.Empty(cards.GetAll());
        Assert.Null(attempts.GetById(attempt.Id)!.UserId);
        Assert.Equal("test_user", attempts.GetById(attempt.Id)!.LoginEntered);
    }

    [Fact]
    public void Transaction_RollbackCancelsRepositorySave()
    {
        using (var transaction = _database.Context.Database.BeginTransaction())
        {
            Repository.Create(TestDatabase.NewUser());
            Assert.Single(Repository.GetAll());
            transaction.Rollback();
        }
        _database.Context.ChangeTracker.Clear();
        Assert.Empty(Repository.GetAll());
    }

}
