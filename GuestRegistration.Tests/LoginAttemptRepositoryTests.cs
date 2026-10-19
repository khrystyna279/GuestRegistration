using GuestRegistration.DAL.Models;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class LoginAttemptRepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private LoginAttemptRepository Repository => new(_database.Context);

    [Fact]
    public void Create_SavesKnownUserAttempt()
    {
        var user = _database.AddUser();
        var attempt = Repository.Create(TestDatabase.NewAttempt(user.Id));
        Assert.True(attempt.Id > 0);
        Assert.True(Repository.GetById(attempt.Id)!.IsSuccessful);
        Assert.Equal(user.Id, Repository.GetById(attempt.Id)!.UserId);
    }

    [Fact]
    public void Create_AllowsNullUserIdForUnknownLogin()
    {
        var attempt = TestDatabase.NewAttempt(null);
        attempt.IsSuccessful = false;
        var saved = Repository.Create(attempt);
        Assert.Null(Repository.GetById(saved.Id)!.UserId);
    }

    [Fact]
    public void GetAll_ReturnsSavedAttempts()
    {
        Repository.Create(TestDatabase.NewAttempt(null));
        Repository.Create(TestDatabase.NewAttempt(null));
        Assert.Equal(2, Repository.GetAll().Count);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingAttempt() => Assert.Null(Repository.GetById(999));

    [Fact]
    public void Update_ChangesPersistedAttempt()
    {
        var attempt = Repository.Create(TestDatabase.NewAttempt(null));
        var changed = Repository.GetById(attempt.Id)!;
        changed.LoginEntered = "changed";
        changed.IsSuccessful = false;
        Assert.True(Repository.Update(changed));
        Assert.Equal("changed", Repository.GetById(attempt.Id)!.LoginEntered);
        Assert.False(Repository.GetById(attempt.Id)!.IsSuccessful);
    }

    [Fact]
    public void Update_ReturnsFalseForMissingAttempt() =>
        Assert.False(Repository.Update(new LoginAttempt { Id = 999 }));

    [Fact]
    public void Delete_RemovesAttempt()
    {
        var attempt = Repository.Create(TestDatabase.NewAttempt(null));
        Assert.True(Repository.Delete(attempt.Id));
        Assert.Null(Repository.GetById(attempt.Id));
        Assert.False(Repository.Delete(attempt.Id));
    }

    [Fact]
    public void Create_RejectsNonexistentUser() =>
        Assert.Throws<DbUpdateException>(() => Repository.Create(TestDatabase.NewAttempt(999)));

    [Fact]
    public void Transaction_RollbackCancelsRepositorySave()
    {
        using (var transaction = _database.Context.Database.BeginTransaction())
        {
            Repository.Create(TestDatabase.NewAttempt(null));
            transaction.Rollback();
        }
        _database.Context.ChangeTracker.Clear();
        Assert.Empty(Repository.GetAll());
    }

    public void Dispose() => _database.Dispose();
}
