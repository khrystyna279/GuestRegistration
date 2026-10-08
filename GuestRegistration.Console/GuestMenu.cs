using GuestRegistration.DAL.Services;

namespace GuestRegistration.ConsoleApp;

internal class GuestMenu
{
    private readonly AccountService _accounts;

    public GuestMenu(AccountService accounts) => _accounts = accounts;

    public void Register()
    {
        Console.WriteLine("\nРеєстрація. Використовуйте вигадані дані та тестову картку.");
        var request = new RegistrationRequest
        {
            Login = ConsoleInput.Read("Логін"),
            FirstName = ConsoleInput.Read("Ім’я"),
            LastName = ConsoleInput.Read("Прізвище"),
            Password = ConsoleInput.ReadSecret("Пароль"),
            ConfirmPassword = ConsoleInput.ReadSecret("Підтвердження пароля"),
            RecoveryWord = ConsoleInput.ReadSecret("Ключове слово для відновлення"),
            Gender = ConsoleInput.Read("Стать (Female / Male / Other)"),
            Address = ConsoleInput.Read("Адреса"),
            Email = ConsoleInput.Read("E-mail"),
            Phone = ConsoleInput.Read("Телефон"),
            CardNumber = ConsoleInput.ReadSecret("Тестовий номер картки (наприклад, 4111111111111111)"),
            CardHolder = ConsoleInput.Read("Ім’я власника картки латиницею"),
            ExpirationMonth = ConsoleInput.ReadInt("Місяць завершення дії (1–12)"),
            ExpirationYear = ConsoleInput.ReadInt("Рік завершення дії (наприклад, 2099)")
        };
        var user = _accounts.Register(request);
        Console.WriteLine($"Реєстрація успішна! ID = {user.Id}, логін = {user.Login}.");
    }

    public void Login()
    {
        string login = ConsoleInput.Read("Логін");
        string password = ConsoleInput.ReadSecret("Пароль");
        var user = _accounts.Login(login, password);
        Console.WriteLine(user == null ? "Неправильний логін або пароль."
            : $"Вхід успішний. Вітаємо, {user.FirstName} {user.LastName}!");
    }

    public void ResetPassword()
    {
        string login = ConsoleInput.Read("Логін");
        string word = ConsoleInput.ReadSecret("Ключове слово");
        string password = ConsoleInput.ReadSecret("Новий пароль");
        string confirmation = ConsoleInput.ReadSecret("Підтвердження нового пароля");
        bool success = _accounts.ResetPassword(login, word, password, confirmation);
        Console.WriteLine(success ? "Пароль змінено. Можна увійти з новим паролем."
            : "Неправильний логін або ключове слово.");
    }
}
