using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using GuestRegistration.DAL.Repositories;

namespace GuestRegistration.DAL.Services;

public class AccountService
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _users;
    private readonly IBankCardRepository _cards;
    private readonly ILoginAttemptRepository _attempts;

    public AccountService(AppDbContext context)
    {
        _context = context;
        _users = new UserRepository(context);
        _cards = new BankCardRepository(context);
        _attempts = new LoginAttemptRepository(context);
    }

    public User Register(RegistrationRequest request)
    {
        InputValidator.ValidatePassword(request.Password, request.ConfirmPassword);
        InputValidator.Required(request.RecoveryWord.Trim(), 100, "Ключове слово");
        var user = new User
        {
            Login = request.Login.Trim().ToLowerInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender.Trim(),
            Address = request.Address.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        InputValidator.ValidateUser(user);
        var card = InputValidator.CreateCard(0, request.CardNumber, request.CardHolder,
            request.ExpirationMonth, request.ExpirationYear);
        if (_users.GetByLogin(user.Login) != null)
            throw new ArgumentException("Цей логін уже використовується.");
        if (_users.GetByEmail(user.Email) != null)
            throw new ArgumentException("Цей e-mail уже використовується.");
        user.PasswordHash = PasswordHasher.Hash(request.Password);
        user.RecoveryWordHash = PasswordHasher.Hash(request.RecoveryWord.Trim().ToLowerInvariant());

        // Both records must be saved together or not saved at all.
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            _users.Create(user);
            card.UserId = user.Id;
            _cards.Create(card);
            transaction.Commit();
            return user;
        }
        catch
        {
            transaction.Rollback();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public User? Login(string login, string password)
    {
        string normalizedLogin = login.Trim().ToLowerInvariant();
        InputValidator.Required(normalizedLogin, 30, "Логін");
        var user = _users.GetByLogin(normalizedLogin);
        bool success = user != null && PasswordHasher.Verify(password, user.PasswordHash);
        _attempts.Create(new LoginAttempt
        {
            UserId = user?.Id,
            LoginEntered = normalizedLogin,
            AttemptedAtUtc = DateTime.UtcNow,
            IsSuccessful = success
        });
        return success ? user : null;
    }

    public bool ResetPassword(string login, string recoveryWord, string newPassword, string confirmation)
    {
        InputValidator.ValidatePassword(newPassword, confirmation);
        var user = _users.GetByLogin(login);
        if (user == null || !PasswordHasher.Verify(recoveryWord.Trim().ToLowerInvariant(), user.RecoveryWordHash))
            return false;
        user.PasswordHash = PasswordHasher.Hash(newPassword);
        return _users.Update(user);
    }
}
