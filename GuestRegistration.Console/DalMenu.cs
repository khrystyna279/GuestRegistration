using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DAL.Models;
using GuestRegistration.DAL.Services;

namespace GuestRegistration.ConsoleApp;

internal class DalMenu
{
    private readonly IUserRepository _users;
    private readonly IBankCardRepository _cards;
    private readonly ILoginAttemptRepository _attempts;
    private readonly GuestMenu _guestMenu;

    public DalMenu(IUserRepository users, IBankCardRepository cards,
        ILoginAttemptRepository attempts, GuestMenu guestMenu)
    {
        _users = users;
        _cards = cards;
        _attempts = attempts;
        _guestMenu = guestMenu;
    }

    public void Run()
    {
        Console.WriteLine("\nДемонстрація DAL. 1 — Users, 2 — BankCards, 3 — LoginAttempts, 0 — назад.");
        string entity = ConsoleInput.Read("Таблиця");
        if (entity == "0") return;
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
    }

    private void UserOperation(string operation)
    {
        switch (operation)
        {
            case "1": _guestMenu.Register(); break;
            case "2":
                foreach (var user in _users.GetAll()) PrintUser(user);
                break;
            case "3": PrintUser(_users.GetById(ConsoleInput.ReadInt("ID"))); break;
            case "4":
                var existing = _users.GetById(ConsoleInput.ReadInt("ID"));
                if (existing == null) { Console.WriteLine("Запис не знайдено."); return; }
                existing.Login = ConsoleInput.Read("Новий логін").Trim().ToLowerInvariant();
                existing.FirstName = ConsoleInput.Read("Ім’я").Trim();
                existing.LastName = ConsoleInput.Read("Прізвище").Trim();
                existing.Gender = ConsoleInput.Read("Стать (Female / Male / Other)").Trim();
                existing.Address = ConsoleInput.Read("Адреса").Trim();
                existing.Email = ConsoleInput.Read("E-mail").Trim().ToLowerInvariant();
                existing.Phone = ConsoleInput.Read("Телефон").Trim();
                InputValidator.ValidateUser(existing);
                PrintResult(_users.Update(existing));
                break;
            case "5":
                int id = ConsoleInput.ReadInt("ID користувача");
                Console.WriteLine("Картки також видаляться; історія входів залишиться без UserId.");
                if (ConsoleInput.ReadBool("Видалити")) PrintResult(_users.Delete(id));
                break;
            default: throw new ArgumentException("Невідома операція.");
        }
    }

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

    private static BankCard ReadCard()
    {
        int userId = ConsoleInput.ReadInt("UserId");
        string number = ConsoleInput.ReadSecret("Тестовий номер (4111111111111111)");
        string holder = ConsoleInput.Read("Власник");
        int month = ConsoleInput.ReadInt("Місяць (1–12)");
        int year = ConsoleInput.ReadInt("Рік (наприклад, 2099)");
        return InputValidator.CreateCard(userId, number, holder, month, year);
    }

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
                changed.AttemptedAtUtc = existing.AttemptedAtUtc;
                PrintResult(_attempts.Update(changed));
                break;
            case "5": PrintResult(_attempts.Delete(ConsoleInput.ReadInt("ID"))); break;
            default: throw new ArgumentException("Невідома операція.");
        }
    }

    private static LoginAttempt ReadAttempt()
    {
        int? userId = ConsoleInput.ReadOptionalId("UserId");
        string login = ConsoleInput.Read("Введений логін").Trim();
        InputValidator.Required(login, 30, "Логін");
        return new LoginAttempt
        {
            UserId = userId,
            LoginEntered = login,
            AttemptedAtUtc = DateTime.UtcNow,
            IsSuccessful = ConsoleInput.ReadBool("Вхід успішний")
        };
    }

    private static void PrintResult(bool success) => Console.WriteLine(success ? "Готово." : "Запис не знайдено.");
    private static void PrintUser(User? user) => Console.WriteLine(user == null ? "Запис не знайдено."
        : $"ID={user.Id} | {user.Login} | {user.FirstName} {user.LastName} | {user.Gender} | {user.Email} | {user.Phone} | {user.Address}");
    private static void PrintCard(BankCard? card) => Console.WriteLine(card == null ? "Запис не знайдено."
        : $"ID={card.Id} | UserId={card.UserId} | {card.Brand} ****{card.LastFourDigits} | {card.CardHolder} | {card.ExpirationMonth:00}/{card.ExpirationYear}");
    private static void PrintAttempt(LoginAttempt? attempt) => Console.WriteLine(attempt == null ? "Запис не знайдено."
        : $"ID={attempt.Id} | UserId={attempt.UserId?.ToString() ?? "NULL"} | {attempt.LoginEntered} | {attempt.AttemptedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Success={attempt.IsSuccessful}");
}
