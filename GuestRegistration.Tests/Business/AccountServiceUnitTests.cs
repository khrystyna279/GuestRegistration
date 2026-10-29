using GuestRegistration.Business.Models;
using GuestRegistration.Business.Services;
using GuestRegistration.DTO;
using Xunit;

namespace GuestRegistration.Tests.Business;

public sealed class AccountServiceUnitTests
{
    private readonly MemorySessions _data = new();
    private readonly UserSession _session = new();
    private readonly FixedClock _clock = new();
    private AccountService Account => new(_data, new TestSecretHasher(), new AccountValidator(_clock), _session, _clock);
    private ProfileService Profiles => new(_data, _session);

    [Fact]
    public void Register_NormalizesAndHashesInputAndStoresOnlyMaskedCard()
    {
        var input = BusinessInputs.Registration() with { Login = " NEW_GUEST ", FirstName = " Test ", Email = " NEW_GUEST@EXAMPLE.COM ", RecoveryWord = " SCHOOL " };
        Assert.True(Account.Register(input).IsSuccess);
        var user = Assert.Single(_data.State.Users);
        Assert.Equal("new_guest", user.Login);
        Assert.Equal("Test", user.FirstName);
        Assert.Equal("new_guest@example.com", user.Email);
        Assert.Equal("Guest", user.RoleCode);
        Assert.Equal(new TestSecretHasher().Hash(input.Password), user.PasswordHash);
        Assert.Equal(new TestSecretHasher().Hash("school"), user.RecoveryWordHash);
        var card = Assert.Single(_data.State.Cards);
        Assert.Equal(user.Id, card.UserId);
        Assert.Equal("1111", card.LastFourDigits);
        Assert.Equal("Visa", card.Brand);
        Assert.True(_data.LastTransactional);
        Assert.Equal(1, _data.Committed);
        Assert.Equal(_data.Opened, _data.Disposed);
        Assert.Null(Account.CurrentUser);
    }

    [Theory]
    [InlineData("Login", "!")]
    [InlineData("FirstName", " ")]
    [InlineData("LastName", "")]
    [InlineData("Password", "short")]
    [InlineData("ConfirmPassword", "different")]
    [InlineData("RecoveryWord", " ")]
    [InlineData("Gender", "unknown")]
    [InlineData("Address", "")]
    [InlineData("Email", "bad")]
    [InlineData("Phone", "letters")]
    [InlineData("CardNumber", "4111111111111112")]
    [InlineData("CardHolder", "123")]
    [InlineData("ExpirationMonth", "0")]
    [InlineData("ExpirationMonth", "13")]
    [InlineData("ExpirationMonth", "x")]
    [InlineData("ExpirationYear", "2025")]
    [InlineData("ExpirationYear", "2101")]
    [InlineData("ExpirationYear", "x")]
    public void Register_InvalidFieldNeverOpensDataSession(string field, string invalid)
    {
        var input = BusinessInputs.Registration();
        typeof(RegistrationInput).GetProperty(field)!.SetValue(input, invalid);
        var result = Account.Register(input);
        Assert.False(result.IsSuccess);
        Assert.Contains(field, result.Errors.Keys);
        Assert.Equal(0, _data.Opened);
    }

    [Theory]
    [InlineData("Login", 31)]
    [InlineData("FirstName", 51)]
    [InlineData("LastName", 51)]
    [InlineData("Password", 101)]
    [InlineData("RecoveryWord", 101)]
    [InlineData("Address", 201)]
    [InlineData("Email", 255)]
    [InlineData("CardHolder", 101)]
    public void Register_RejectsOverlongFields(string field, int length)
    {
        var input = BusinessInputs.Registration();
        typeof(RegistrationInput).GetProperty(field)!.SetValue(input, new string('a', length));
        Assert.Contains(field, Account.Register(input).Errors.Keys);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Register_RejectsDuplicateLoginOrEmail(bool login)
    {
        var existing = _data.SeedUser();
        var input = BusinessInputs.Registration();
        input = login ? input with { Login = existing.Login.ToUpperInvariant() } : input with { Email = existing.Email.ToUpperInvariant() };
        var result = Account.Register(input);
        Assert.False(result.IsSuccess);
        Assert.Contains(login ? "Login" : "Email", result.Errors.Keys);
        Assert.Single(_data.State.Users);
        Assert.Empty(_data.State.Cards);
        Assert.Equal(0, _data.Committed);
        Assert.Equal(_data.Opened, _data.Disposed);
    }

    [Fact]
    public void Register_CardFailureDoesNotCommitTheUser()
    {
        _data.FailCardCreate = true;
        Assert.False(Account.Register(BusinessInputs.Registration()).IsSuccess);
        Assert.Empty(_data.State.Users);
        Assert.Empty(_data.State.Cards);
        Assert.Equal(0, _data.Committed);
        Assert.Equal(_data.Opened, _data.Disposed);
    }

    [Theory]
    [InlineData("Guest", true)]
    [InlineData("Restricted", false)]
    [InlineData("OtherRole", false)]
    [InlineData("", false)]
    public void Login_AuthenticatesButGrantsAccessOnlyToGuest(string role, bool allowed)
    {
        var seeded = _data.SeedUser(role: role);
        var result = Account.Login(new(" GUEST_USER ", "Student123!"));
        Assert.True(result.IsSuccess);
        Assert.Equal(allowed, result.User!.HasGuestAccess);
        Assert.Equal(seeded.Id, Account.CurrentUser!.Id);
        Assert.True(Assert.Single(_data.State.Attempts).IsSuccessful);
        if (allowed) Assert.Equal("guest_user", Profiles.GetCurrentProfile().Login);
        else Assert.Throws<UnauthorizedAccessException>(() => Profiles.GetCurrentProfile());
    }

    [Theory]
    [InlineData("guest_user", "wrong", 1)]
    [InlineData("unknown", "Student123!", 0)]
    public void Login_WrongCredentialsLogFailureAndCreateNoSession(string login, string password, int userId)
    {
        _data.SeedUser();
        Assert.False(Account.Login(new(login, password)).IsSuccess);
        Assert.Null(Account.CurrentUser);
        var attempt = Assert.Single(_data.State.Attempts);
        Assert.False(attempt.IsSuccessful);
        Assert.Equal(userId == 0 ? null : (int?)userId, attempt.UserId);
    }

    [Fact]
    public void Login_InvalidInputDoesNotOpenDatabase()
    {
        Assert.False(Account.Login(new("!", "")).IsSuccess);
        Assert.Equal(0, _data.Opened);
    }

    [Fact]
    public void FailedLoginClearsPreviousSession()
    {
        _data.SeedUser(); Account.Login(new("guest_user", "Student123!"));
        Account.Login(new("guest_user", "wrong"));
        Assert.Null(Account.CurrentUser);
    }

    [Fact]
    public void AnonymousUserCannotReadProfile()
    {
        Assert.Throws<UnauthorizedAccessException>(() => Profiles.GetCurrentProfile());
        Assert.Equal(0, _data.Opened);
    }

    [Fact]
    public void ProfileContainsOnlyCurrentUsersCardsAndNoHashes()
    {
        var first = _data.SeedUser(); var other = _data.SeedUser("other_user");
        _data.State.Cards.Add(new BankCardDto { Id = 1, UserId = first.Id, LastFourDigits = "1111" });
        _data.State.Cards.Add(new BankCardDto { Id = 2, UserId = other.Id, LastFourDigits = "2222" });
        Account.Login(new(first.Login, "Student123!"));
        var profile = Profiles.GetCurrentProfile();
        Assert.Equal(first.Email, profile.Email);
        Assert.Equal("1111", Assert.Single(profile.Cards).LastFourDigits);
        Assert.DoesNotContain("Hash", string.Join(" ", profile.GetType().GetProperties().Select(property => property.Name)));
    }

    [Fact]
    public void RevokedRoleIsRecheckedByBusinessLayer()
    {
        _data.SeedUser(); Account.Login(new("guest_user", "Student123!"));
        _data.State.Users[0].RoleCode = "Restricted";
        Assert.Throws<UnauthorizedAccessException>(() => Profiles.GetCurrentProfile());
    }

    [Fact]
    public void SignedInUserMustLogoutBeforeRegisteringOrRecovering()
    {
        _data.SeedUser(); Account.Login(new("guest_user", "Student123!"));
        int opened = _data.Opened;
        Assert.False(Account.Register(BusinessInputs.Registration()).IsSuccess);
        Assert.False(Account.RecoverPassword(new("guest_user", "school", "NewPass123!", "NewPass123!")).IsSuccess);
        Assert.Equal(opened, _data.Opened);
    }

    [Fact]
    public void LogoutRemovesAccess()
    {
        _data.SeedUser(); Account.Login(new("guest_user", "Student123!")); Account.Logout();
        Assert.Null(Account.CurrentUser);
        Assert.Throws<UnauthorizedAccessException>(() => Profiles.GetCurrentProfile());
    }

    [Fact]
    public void RecoverPasswordNormalizesWordAndReplacesOnlyPassword()
    {
        var before = _data.SeedUser();
        var result = Account.RecoverPassword(new(" GUEST_USER ", " SCHOOL ", "NewPass123!", "NewPass123!"));
        Assert.True(result.IsSuccess);
        var after = Assert.Single(_data.State.Users);
        Assert.Equal(before.RecoveryWordHash, after.RecoveryWordHash);
        Assert.Equal(before.RoleCode, after.RoleCode);
        Assert.False(Account.Login(new("guest_user", "Student123!")).IsSuccess);
        Assert.True(Account.Login(new("guest_user", "NewPass123!")).IsSuccess);
    }

    [Theory]
    [InlineData("unknown", "school")]
    [InlineData("guest_user", "wrong")]
    public void RecoverPasswordRejectsWrongCredentials(string login, string word)
    {
        var before = _data.SeedUser();
        Assert.False(Account.RecoverPassword(new(login, word, "NewPass123!", "NewPass123!")).IsSuccess);
        Assert.Equal(before.PasswordHash, _data.State.Users[0].PasswordHash);
        Assert.Equal(0, _data.Committed);
    }

    [Theory]
    [InlineData("!", "school", "NewPass123!", "NewPass123!")]
    [InlineData("guest_user", "", "NewPass123!", "NewPass123!")]
    [InlineData("guest_user", "school", "short", "short")]
    [InlineData("guest_user", "school", "NewPass123!", "different")]
    public void RecoverPasswordValidatesInputBeforeOpeningDatabase(string login, string word, string password, string confirmation)
    {
        Assert.False(Account.RecoverPassword(new(login, word, password, confirmation)).IsSuccess);
        Assert.Equal(0, _data.Opened);
    }
}
