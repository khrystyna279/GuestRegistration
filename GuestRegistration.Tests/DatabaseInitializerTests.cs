using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class DatabaseInitializerTests
{
    [SqlServerFact]
    public void Initialize_SeedsTwentyRowsPerTableAndDoesNotDuplicateData()
    {
        using var database = new TestDatabase();
        database.Seed();
        Assert.Equal(20, database.Context.Users.Count());
        Assert.Equal(20, database.Context.BankCards.Count());
        Assert.Equal(20, database.Context.LoginAttempts.Count());
        var user = database.Context.Users.AsNoTracking().First(user => user.Login == "guest01");
        Assert.True(PasswordHasher.Verify("Student123!", user.PasswordHash));
        database.Seed();
        Assert.Equal(20, database.Context.Users.Count());
        Assert.Equal(20, database.Context.BankCards.Count());
        Assert.Equal(20, database.Context.LoginAttempts.Count());
        Assert.All(database.Context.BankCards.AsNoTracking().ToList(), card =>
            Assert.True(database.Context.Users.Any(user => user.Id == card.UserId)));
    }
}
