using GuestRegistration.Business.Services;
using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests.Business;

public sealed class AccountIntegrationTests
{
    [Fact]
    public void RegisterLoginRecoverAndRoleRestrictionWorkOnSqlServer()
    {
        using var database = new TestDatabase();
        var factory = new DataSessionFactory(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(database.Context.Database.GetConnectionString()).Options);
        var clock = new FixedClock(); var session = new UserSession();
        var accounts = new AccountService(factory, new Pbkdf2SecretHasher(), new AccountValidator(clock), session, clock);
        var profiles = new ProfileService(factory, session);
        var result = accounts.Register(BusinessInputs.Registration());
        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Values));
        Assert.Single(new UserRepository(database.Context).GetAll());
        Assert.Single(new BankCardRepository(database.Context).GetAll());
        Assert.True(accounts.Login(new("new_guest", "Student123!")).IsSuccess);
        Assert.Equal("1111", Assert.Single(profiles.GetCurrentProfile().Cards).LastFourDigits);
        accounts.Logout();
        Assert.True(accounts.RecoverPassword(new("new_guest", "school", "NewPass123!", "NewPass123!")).IsSuccess);
        Assert.False(accounts.Login(new("new_guest", "Student123!")).IsSuccess);
        Assert.True(accounts.Login(new("new_guest", "NewPass123!")).IsSuccess);
        var user = new UserRepository(database.Context).FindByLogin("new_guest")!;
        user.RoleCode = "Restricted";
        new UserRepository(database.Context).Update(user);
        Assert.Throws<UnauthorizedAccessException>(() => profiles.GetCurrentProfile());
        accounts.Logout();
        Assert.False(accounts.Login(new("new_guest", "NewPass123!")).User!.HasGuestAccess);
    }

    [Fact]
    public void UncommittedDataSessionRollsBackRepositoryWrites()
    {
        using var database = new TestDatabase();
        var factory = new DataSessionFactory(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(database.Context.Database.GetConnectionString()).Options);
        using (var data = factory.Open(transactional: true))
        {
            var user = data.Users.Create(TestDatabase.NewUser());
            data.Cards.Create(TestDatabase.NewCard(user.Id));
        }
        Assert.Empty(new UserRepository(database.Context).GetAll());
        Assert.Empty(new BankCardRepository(database.Context).GetAll());
    }

    [Fact]
    public void CommittedDataSessionPersistsRepositoryWrites()
    {
        using var database = new TestDatabase();
        var factory = new DataSessionFactory(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(database.Context.Database.GetConnectionString()).Options);
        using (var data = factory.Open(transactional: true))
        {
            data.Users.Create(TestDatabase.NewUser()); data.Commit();
        }
        Assert.Single(new UserRepository(database.Context).GetAll());
    }
}
