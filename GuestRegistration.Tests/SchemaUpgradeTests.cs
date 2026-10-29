using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public sealed class SchemaUpgradeTests
{
    [Fact]
    public void RoleUpgrade_PreservesCredentialsAndCanBeRepeated()
    {
        using var database = new TestDatabase();
        var repository = new UserRepository(database.Context);
        var saved = repository.Create(TestDatabase.NewUser());
        database.Context.ChangeTracker.Clear();
        database.Context.Database.ExecuteSqlRaw("ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_RoleCode; ALTER TABLE dbo.Users DROP COLUMN RoleCode;");
        string upgrade = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "sql", "07_AddUserRole.sql"));
        database.Context.Database.ExecuteSqlRaw(upgrade);
        database.Context.Database.ExecuteSqlRaw(upgrade);
        var user = repository.GetById(saved.Id)!;
        Assert.Equal(saved.PasswordHash, user.PasswordHash);
        Assert.Equal(saved.RecoveryWordHash, user.RecoveryWordHash);
        Assert.Equal(saved.Email, user.Email);
        Assert.Equal("Guest", user.RoleCode);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Upgrade_PreservesExistingDataAndCanBeRepeated()
    {
        using var database = new TestDatabase();
        var users = new UserRepository(database.Context);
        var cards = new BankCardRepository(database.Context);
        var attempts = new LoginAttemptRepository(database.Context);
        var user = users.Create(TestDatabase.NewUser());
        var card = cards.Create(TestDatabase.NewCard(user.Id));
        var attempt = attempts.Create(TestDatabase.NewAttempt(user.Id));
        database.Context.ChangeTracker.Clear();

        // Reproduce the previous schema only inside this test's temporary database.
        foreach (string table in new[] { "Users", "BankCards", "LoginAttempts" })
        {
            string sql = $"DROP TRIGGER dbo.TR_{table}_AuditTime; "
                + $"ALTER TABLE dbo.{table} DROP CONSTRAINT DF_{table}_RowInsertTime, DF_{table}_UpdateTime; "
                + $"ALTER TABLE dbo.{table} DROP COLUMN RowInsertTime, UpdateTime;";
            database.Context.Database.ExecuteSqlRaw(sql);
        }

        string upgrade = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "sql", "06_AddAuditColumns.sql"));
        database.Context.Database.ExecuteSqlRaw(upgrade);
        database.Context.Database.ExecuteSqlRaw(upgrade);

        Assert.Single(users.GetAll());
        Assert.Single(cards.GetAll());
        Assert.Single(attempts.GetAll());
        Assert.Equal(user.Login, users.GetById(user.Id)!.Login);
        Assert.Equal(user.Id, cards.GetById(card.Id)!.UserId);
        Assert.Equal(user.Id, attempts.GetById(attempt.Id)!.UserId);
        Assert.True(users.GetById(user.Id)!.RowInsertTime > DateTime.UnixEpoch);
        var changed = cards.GetById(card.Id)!;
        changed.CardHolder = "AFTER UPGRADE";
        cards.Update(changed);
        Assert.True(cards.GetById(card.Id)!.UpdateTime > changed.UpdateTime);
    }
}
