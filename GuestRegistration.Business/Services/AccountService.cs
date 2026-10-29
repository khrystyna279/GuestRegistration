using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.Business.Services;

public sealed class AccountService : IAccountService
{
    private readonly IDataSessionFactory _data;
    private readonly ISecretHasher _hasher;
    private readonly IAccountValidator _validator;
    private readonly UserSession _session;
    private readonly TimeProvider _clock;
    public AuthenticatedUser? CurrentUser => _session.Current;

    public AccountService(IDataSessionFactory data, ISecretHasher hasher, IAccountValidator validator, UserSession session, TimeProvider clock)
    { _data = data; _hasher = hasher; _validator = validator; _session = session; _clock = clock; }

    public AccountResult Register(RegistrationInput input)
    {
        if (CurrentUser != null) return AccountResult.Failure("Спочатку вийдіть з облікового запису.");
        var errors = _validator.Registration(input);
        if (errors.Count != 0) return AccountResult.Invalid(errors);
        string login = AccountValidator.NormalizeLogin(input.Login);
        string email = input.Email.Trim().ToLowerInvariant();
        using var data = _data.Open(transactional: true);
        if (data.Users.FindByLogin(login) != null) return AccountResult.Failure("Цей логін уже використовується.", "Login");
        if (data.Users.FindByEmail(email) != null) return AccountResult.Failure("Цей e-mail уже використовується.", "Email");
        try
        {
            var user = data.Users.Create(new UserDto
            {
                Login = login, FirstName = input.FirstName.Trim(), LastName = input.LastName.Trim(),
                PasswordHash = _hasher.Hash(input.Password), RecoveryWordHash = _hasher.Hash(AccountValidator.NormalizeWord(input.RecoveryWord)),
                RoleCode = "Guest", Gender = input.Gender, Address = input.Address.Trim(), Email = email,
                Phone = input.Phone.Trim(), CreatedAtUtc = _clock.GetUtcNow().UtcDateTime
            });
            string digits = AccountValidator.CardDigits(input.CardNumber);
            data.Cards.Create(new BankCardDto
            {
                UserId = user.Id, CardHolder = input.CardHolder.Trim().ToUpperInvariant(),
                Brand = digits.StartsWith('4') ? "Visa" : digits.StartsWith('5') || int.Parse(digits[..4]) is >= 2221 and <= 2720 ? "Mastercard" : "Other",
                LastFourDigits = digits[^4..], ExpirationMonth = int.Parse(input.ExpirationMonth), ExpirationYear = int.Parse(input.ExpirationYear)
            });
            data.Commit();
            return AccountResult.Success();
        }
        catch (DbUpdateException) { return AccountResult.Failure("Не вдалося зберегти обліковий запис. Перевірте дані; логін та e-mail мають бути унікальними."); }
    }

    public AccountResult Login(LoginInput input)
    {
        _session.Clear();
        var errors = _validator.Login(input);
        if (errors.Count != 0) return AccountResult.Invalid(errors);
        string login = AccountValidator.NormalizeLogin(input.Login);
        using var data = _data.Open();
        var user = data.Users.FindByLogin(login);
        bool valid = user != null && _hasher.Verify(input.Password, user.PasswordHash);
        data.Attempts.Create(new LoginAttemptDto
        {
            UserId = user?.Id, LoginEntered = login, AttemptedAtUtc = _clock.GetUtcNow().UtcDateTime, IsSuccessful = valid
        });
        data.Commit();
        if (!valid) return AccountResult.Failure("Неправильний логін або пароль.");
        var authenticated = new AuthenticatedUser(user!.Id, user.Login, user.FirstName + " " + user.LastName, user.RoleCode == "Guest");
        _session.Start(authenticated);
        return AccountResult.Success(authenticated);
    }

    public AccountResult RecoverPassword(RecoveryInput input)
    {
        if (CurrentUser != null) return AccountResult.Failure("Спочатку вийдіть з облікового запису.");
        var errors = _validator.Recovery(input);
        if (errors.Count != 0) return AccountResult.Invalid(errors);
        using var data = _data.Open(transactional: true);
        var user = data.Users.FindByLogin(AccountValidator.NormalizeLogin(input.Login));
        if (user == null || !_hasher.Verify(AccountValidator.NormalizeWord(input.RecoveryWord), user.RecoveryWordHash))
            return AccountResult.Failure("Неправильний логін або ключове слово.");
        user.PasswordHash = _hasher.Hash(input.Password);
        if (!data.Users.Update(user)) return AccountResult.Failure("Обліковий запис більше не доступний.");
        data.Commit();
        return AccountResult.Success();
    }

    public void Logout() => _session.Clear();
}
