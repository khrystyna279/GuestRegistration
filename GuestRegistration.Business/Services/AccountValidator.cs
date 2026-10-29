using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;

namespace GuestRegistration.Business.Services;

public sealed class AccountValidator : IAccountValidator
{
    private readonly TimeProvider _clock;
    public AccountValidator(TimeProvider clock) => _clock = clock;

    public IReadOnlyDictionary<string, string> Login(LoginInput input)
    {
        var errors = new Dictionary<string, string>();
        CheckLogin(errors, input.Login);
        if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length > 100)
            errors["Password"] = "Введіть пароль (до 100 символів).";
        return errors;
    }

    public IReadOnlyDictionary<string, string> Recovery(RecoveryInput input)
    {
        var errors = new Dictionary<string, string>();
        CheckLogin(errors, input.Login);
        Required(errors, "RecoveryWord", input.RecoveryWord, 100, "Ключове слово");
        CheckPassword(errors, input.Password, input.ConfirmPassword);
        return errors;
    }

    public IReadOnlyDictionary<string, string> Registration(RegistrationInput input)
    {
        var errors = new Dictionary<string, string>();
        CheckLogin(errors, input.Login);
        Required(errors, "FirstName", input.FirstName, 50, "Ім’я");
        Required(errors, "LastName", input.LastName, 50, "Прізвище");
        Required(errors, "Address", input.Address, 200, "Адреса");
        Required(errors, "RecoveryWord", input.RecoveryWord, 100, "Ключове слово");
        CheckPassword(errors, input.Password, input.ConfirmPassword);
        if (input.Gender is not ("Female" or "Male" or "Other")) errors["Gender"] = "Оберіть стать.";
        string email = input.Email.Trim().ToLowerInvariant();
        if (email.Length > 254 || !MailAddress.TryCreate(email, out var address)
            || address.Address != email || !address.Host.Contains('.')) errors["Email"] = "Введіть коректний e-mail.";
        if (!Regex.IsMatch(input.Phone.Trim(), @"^\+?[0-9]{7,15}$")) errors["Phone"] = "Телефон: 7–15 цифр, можна з + на початку.";
        string digits = CardDigits(input.CardNumber);
        if (!Regex.IsMatch(digits, @"^[0-9]{16}$") || !IsLuhnValid(digits)) errors["CardNumber"] = "Введіть коректний 16-значний навчальний номер картки.";
        if (!Regex.IsMatch(input.CardHolder.Trim(), @"^[A-Za-z][A-Za-z '\-]{0,99}$")) errors["CardHolder"] = "Власник картки: латиницею, до 100 символів.";
        bool monthValid = int.TryParse(input.ExpirationMonth, NumberStyles.None, CultureInfo.InvariantCulture, out int month) && month is >= 1 and <= 12;
        bool yearValid = int.TryParse(input.ExpirationYear, NumberStyles.None, CultureInfo.InvariantCulture, out int year) && year <= 2100 && year >= _clock.GetUtcNow().Year;
        if (!monthValid) errors["ExpirationMonth"] = "Місяць має бути від 1 до 12.";
        if (!yearValid) errors["ExpirationYear"] = "Вкажіть поточний або майбутній рік, до 2100.";
        if (monthValid && yearValid && year == _clock.GetUtcNow().Year && month < _clock.GetUtcNow().Month)
            errors["ExpirationMonth"] = "Термін дії картки вже минув.";
        return errors;
    }

    public static string NormalizeLogin(string value) => value.Trim().ToLowerInvariant();
    public static string NormalizeWord(string value) => value.Trim().ToLowerInvariant();
    public static string CardDigits(string value) => value.Replace(" ", "").Replace("-", "");

    private static void CheckLogin(Dictionary<string, string> errors, string login)
    {
        if (!Regex.IsMatch(NormalizeLogin(login), @"^[a-z0-9_]{3,30}$"))
            errors["Login"] = "Логін: 3–30 латинських літер, цифр або _.";
    }

    private static void Required(Dictionary<string, string> errors, string key, string value, int limit, string label)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > limit) errors[key] = $"{label}: обов’язкове поле, до {limit} символів.";
    }

    private static void CheckPassword(Dictionary<string, string> errors, string password, string confirmation)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length is < 8 or > 100) errors["Password"] = "Пароль має містити 8–100 символів.";
        if (string.IsNullOrWhiteSpace(confirmation) || password != confirmation) errors["ConfirmPassword"] = "Підтвердження має збігатися з паролем.";
    }

    private static bool IsLuhnValid(string digits)
    {
        int sum = 0;
        for (int i = 0; i < digits.Length; i++)
        {
            int digit = digits[digits.Length - 1 - i] - '0';
            if (i % 2 == 1) { digit *= 2; if (digit > 9) digit -= 9; }
            sum += digit;
        }
        return sum % 10 == 0;
    }
}
