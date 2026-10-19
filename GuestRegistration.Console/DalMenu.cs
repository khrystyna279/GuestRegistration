using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;

namespace GuestRegistration.ConsoleApp;

internal class DalMenu
{
    private readonly IUserRepository _users;
    private readonly IBankCardRepository _cards;
    private readonly ILoginAttemptRepository _attempts;

    public DalMenu(IUserRepository users, IBankCardRepository cards,
        ILoginAttemptRepository attempts)
    {
        _users = users;
        _cards = cards;
        _attempts = attempts;
    }

    public bool Run()
    {
        Console.WriteLine("1 — Users, 2 — BankCards, 3 — LoginAttempts, 0 — вихід.");
        string entity = ConsoleInput.Read("Таблиця");
        if (entity == "0") return false;
        if (entity != "1" && entity != "2" && entity != "3")
            throw new ArgumentException("Невідома таблиця.");
        Console.WriteLine("1 — Create, 2 — Read all, 3 — Read by ID, 4 — Update, 5 — Delete.");
        string operation = ConsoleInput.Read("Операція");
        switch (entity)
        {
            case "1": UserOperation(operation); break;
            case "2": CardOperation(operation); break;
            case "3": AttemptOperation(operation); break;
        }
        return true;
    }

    private void UserOperation(string operation)
    {
        switch (operation)
        {
            case "1": PrintUser(_users.Create(ReadUser())); break;
            case "2":
                foreach (var user in _users.GetAll()) PrintUser(user);
                break;
            case "3": PrintUser(_users.GetById(ConsoleInput.ReadInt("ID"))); break;
            case "4":
                var existing = _users.GetById(ConsoleInput.ReadInt("ID"));
                if (existing == null) { Console.WriteLine("Запис не знайдено."); return; }
                var changed = ReadUser();
                changed.Id = existing.Id;
                PrintResult(_users.Update(changed));
                break;
            case "5":
                int id = ConsoleInput.ReadInt("ID користувача");
                Console.WriteLine("Картки також видаляться; історія входів залишиться без UserId.");
                if (ConsoleInput.ReadBool("Видалити")) PrintResult(_users.Delete(id));
                break;
            default: throw new ArgumentException("Невідома операція.");
        }
    }

    private static User ReadUser() => new()
    {
        Login = ConsoleInput.Read("Login (латиниця в нижньому регістрі)"),
        FirstName = ConsoleInput.Read("FirstName"),
        LastName = ConsoleInput.Read("LastName"),
        PasswordHash = ConsoleInput.Read("PasswordHash (готове навчальне значення)"),
        RecoveryWordHash = ConsoleInput.Read("RecoveryWordHash (готове навчальне значення)"),
        Gender = ConsoleInput.Read("Gender (Female / Male / Other)"),
        Address = ConsoleInput.Read("Address"),
        Email = ConsoleInput.Read("Email (у нижньому регістрі)"),
        Phone = ConsoleInput.Read("Phone"),
        CreatedAtUtc = ConsoleInput.ReadUtcDateTime("CreatedAtUtc")
    };

    private void CardOperation(string operation)
    {
        switch (operation)
        {
            case "1": PrintCard(_cards.Create(ReadCard())); break;
            case "2":
                foreach (var card in _cards.GetAll()) PrintCard(card);
                break;
            case "3": PrintCard(_cards.GetById(ConsoleInput.ReadInt("ID"))); break;
            case "4":
                int id = ConsoleInput.ReadInt("ID картки");
                if (_cards.GetById(id) == null) { Console.WriteLine("Запис не знайдено."); return; }
                var changed = ReadCard();
                changed.Id = id;
                PrintResult(_cards.Update(changed));
                break;
            case "5": PrintResult(_cards.Delete(ConsoleInput.ReadInt("ID"))); break;
            default: throw new ArgumentException("Невідома операція.");
        }
    }

    private static BankCard ReadCard() => new()
    {
        UserId = ConsoleInput.ReadInt("UserId"),
        CardHolder = ConsoleInput.Read("CardHolder"),
        Brand = ConsoleInput.Read("Brand (Visa / Mastercard / Other)"),
        LastFourDigits = ConsoleInput.Read("LastFourDigits (4 цифри)"),
        ExpirationMonth = ConsoleInput.ReadInt("ExpirationMonth (1–12)"),
        ExpirationYear = ConsoleInput.ReadInt("ExpirationYear (2020–2100)")
    };

    private void AttemptOperation(string operation)
    {
        switch (operation)
        {
            case "1": PrintAttempt(_attempts.Create(ReadAttempt())); break;
            case "2":
                foreach (var attempt in _attempts.GetAll()) PrintAttempt(attempt);
                break;
            case "3": PrintAttempt(_attempts.GetById(ConsoleInput.ReadInt("ID"))); break;
            case "4":
                int id = ConsoleInput.ReadInt("ID спроби");
                var existing = _attempts.GetById(id);
                if (existing == null) { Console.WriteLine("Запис не знайдено."); return; }
                var changed = ReadAttempt();
                changed.Id = id;
                PrintResult(_attempts.Update(changed));
                break;
            case "5": PrintResult(_attempts.Delete(ConsoleInput.ReadInt("ID"))); break;
            default: throw new ArgumentException("Невідома операція.");
        }
    }

    private static LoginAttempt ReadAttempt()
    {
        int? userId = ConsoleInput.ReadOptionalId("UserId");
        string login = ConsoleInput.Read("LoginEntered");
        DateTime attemptedAtUtc = ConsoleInput.ReadUtcDateTime("AttemptedAtUtc");
        return new LoginAttempt
        {
            UserId = userId,
            LoginEntered = login,
            AttemptedAtUtc = attemptedAtUtc,
            IsSuccessful = ConsoleInput.ReadBool("IsSuccessful")
        };
    }

    private static void PrintResult(bool success) => Console.WriteLine(success ? "Готово." : "Запис не знайдено.");
    private static void PrintUser(User? user) => Console.WriteLine(user == null ? "Запис не знайдено."
        : $"ID={user.Id} | {user.Login} | {user.FirstName} {user.LastName} | {user.Gender} | {user.Email} | {user.Phone} | {user.Address} | PasswordHash={user.PasswordHash} | RecoveryWordHash={user.RecoveryWordHash} | CreatedAtUtc={user.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
    private static void PrintCard(BankCard? card) => Console.WriteLine(card == null ? "Запис не знайдено."
        : $"ID={card.Id} | UserId={card.UserId} | {card.Brand} ****{card.LastFourDigits} | {card.CardHolder} | {card.ExpirationMonth:00}/{card.ExpirationYear}");
    private static void PrintAttempt(LoginAttempt? attempt) => Console.WriteLine(attempt == null ? "Запис не знайдено."
        : $"ID={attempt.Id} | UserId={attempt.UserId?.ToString() ?? "NULL"} | {attempt.LoginEntered} | {attempt.AttemptedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Success={attempt.IsSuccessful}");
}
