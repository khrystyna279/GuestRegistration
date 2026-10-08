using GuestRegistration.DAL.Repositories;
using GuestRegistration.DAL.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuestRegistration.Tests;

public class AccountServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private AccountService Service => new(_database.Context);

    [SqlServerFact]
    public void Register_SavesNormalizedUserAndMaskedCard()
    {
        var request = TestDatabase.NewRegistration();
        request.Login = " NEW_GUEST ";
        request.Email = " NEW_GUEST@EXAMPLE.COM ";
        var user = Service.Register(request);
        Assert.Equal("new_guest", user.Login);
        Assert.Equal("new_guest@example.com", user.Email);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.True(PasswordHasher.Verify(request.Password, user.PasswordHash));
        Assert.True(PasswordHasher.Verify("school", user.RecoveryWordHash));
        var card = Assert.Single(new BankCardRepository(_database.Context).GetAll());
        Assert.Equal(user.Id, card.UserId);
        Assert.Equal("1111", card.LastFourDigits);
    }

    [SqlServerTheory]
    [InlineData("password")]
    [InlineData("confirmation")]
    [InlineData("login")]
    [InlineData("email")]
    [InlineData("phone")]
    [InlineData("gender")]
    [InlineData("word")]
    [InlineData("name")]
    [InlineData("address")]
    [InlineData("card")]
    [InlineData("month")]
    [InlineData("year")]
    public void Register_InvalidInputSavesNothing(string field)
    {
        var request = TestDatabase.NewRegistration();
        switch (field)
        {
            case "password": request.Password = request.ConfirmPassword = "short"; break;
            case "confirmation": request.ConfirmPassword = "different"; break;
            case "login": request.Login = "bad login"; break;
            case "email": request.Email = "invalid"; break;
            case "phone": request.Phone = "phone"; break;
            case "gender": request.Gender = "invalid"; break;
            case "word": request.RecoveryWord = " "; break;
            case "name": request.FirstName = " "; break;
            case "address": request.Address = " "; break;
            case "card": request.CardNumber = "4111111111111112"; break;
            case "month": request.ExpirationMonth = 13; break;
            case "year": request.ExpirationYear = 2020; break;
        }
        Assert.Throws<ArgumentException>(() => Service.Register(request));
        Assert.Empty(_database.Context.Users.AsNoTracking());
        Assert.Empty(_database.Context.BankCards.AsNoTracking());
    }

    [SqlServerTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Register_RejectsDuplicateLoginOrEmail(bool duplicateLogin)
    {
        Service.Register(TestDatabase.NewRegistration());
        var duplicate = TestDatabase.NewRegistration();
        if (duplicateLogin) duplicate.Email = "other@example.com";
        else duplicate.Login = "other_guest";
        Assert.Throws<ArgumentException>(() => Service.Register(duplicate));
        Assert.Single(_database.Context.Users.AsNoTracking());
        Assert.Single(_database.Context.BankCards.AsNoTracking());
    }

    [SqlServerFact]
    public void Register_CardSaveFailureRollsBackUserAndAllowsRetry()
    {
        // Force the second database write to fail after the user has been saved.
        _database.Context.Database.ExecuteSqlRaw("ALTER TABLE dbo.BankCards ADD CONSTRAINT CK_TestRejectCard CHECK (1 = 0);");
        Assert.Throws<DbUpdateException>(() => Service.Register(TestDatabase.NewRegistration()));
        Assert.Empty(_database.Context.Users.AsNoTracking());
        Assert.Empty(_database.Context.BankCards.AsNoTracking());
        _database.Context.Database.ExecuteSqlRaw("ALTER TABLE dbo.BankCards DROP CONSTRAINT CK_TestRejectCard;");
        var user = Service.Register(TestDatabase.NewRegistration());
        Assert.True(user.Id > 0);
        Assert.Single(_database.Context.Users.AsNoTracking());
    }

    [SqlServerFact]
    public void Login_CorrectPasswordReturnsUserAndLogsSuccess()
    {
        var user = Service.Register(TestDatabase.NewRegistration());
        var loggedIn = Service.Login(" NEW_GUEST ", "Student123!");
        Assert.Equal(user.Id, loggedIn!.Id);
        var attempt = Assert.Single(_database.Context.LoginAttempts.AsNoTracking());
        Assert.True(attempt.IsSuccessful);
        Assert.Equal(user.Id, attempt.UserId);
    }

    [SqlServerFact]
    public void Login_WrongPasswordLogsFailure()
    {
        var user = Service.Register(TestDatabase.NewRegistration());
        Assert.Null(Service.Login(user.Login, "wrong"));
        var attempt = Assert.Single(_database.Context.LoginAttempts.AsNoTracking());
        Assert.False(attempt.IsSuccessful);
        Assert.Equal(user.Id, attempt.UserId);
    }

    [SqlServerFact]
    public void Login_UnknownUserLogsFailureWithNullForeignKey()
    {
        Assert.Null(Service.Login("unknown", "wrong"));
        var attempt = Assert.Single(_database.Context.LoginAttempts.AsNoTracking());
        Assert.Null(attempt.UserId);
        Assert.False(attempt.IsSuccessful);
    }

    [SqlServerFact]
    public void ResetPassword_CorrectWordReplacesPassword()
    {
        Service.Register(TestDatabase.NewRegistration());
        Assert.True(Service.ResetPassword("new_guest", " SCHOOL ", "NewPassword123!", "NewPassword123!"));
        Assert.Null(Service.Login("new_guest", "Student123!"));
        Assert.NotNull(Service.Login("new_guest", "NewPassword123!"));
    }

    [SqlServerTheory]
    [InlineData("new_guest", "wrong")]
    [InlineData("unknown", "school")]
    public void ResetPassword_WrongCredentialsLeavePasswordUnchanged(string login, string word)
    {
        var user = Service.Register(TestDatabase.NewRegistration());
        Assert.False(Service.ResetPassword(login, word, "NewPassword123!", "NewPassword123!"));
        Assert.Equal(user.PasswordHash, new UserRepository(_database.Context).GetById(user.Id)!.PasswordHash);
    }

    [SqlServerFact]
    public void ResetPassword_MismatchedConfirmationLeavesPasswordUnchanged()
    {
        var user = Service.Register(TestDatabase.NewRegistration());
        Assert.Throws<ArgumentException>(() => Service.ResetPassword(user.Login, "school", "NewPassword123!", "different"));
        Assert.Equal(user.PasswordHash, new UserRepository(_database.Context).GetById(user.Id)!.PasswordHash);
    }

    public void Dispose() => _database.Dispose();
}
