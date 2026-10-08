using System.Net.Mail;
using System.Text.RegularExpressions;
using GuestRegistration.DAL.Models;

namespace GuestRegistration.DAL.Services;

public static class InputValidator
{
    public static void ValidateUser(User user)
    {
        if (!Regex.IsMatch(user.Login, @"^[a-z0-9_]{3,30}$"))
            throw new ArgumentException("Логін: 3–30 латинських літер, цифр або символів _.");
        Required(user.FirstName, 50, "Ім’я");
        Required(user.LastName, 50, "Прізвище");
        Required(user.Address, 200, "Адреса");
        if (user.Gender != "Female" && user.Gender != "Male" && user.Gender != "Other")
            throw new ArgumentException("Стать: Female, Male або Other.");
        if (user.Email.Length > 254 || !MailAddress.TryCreate(user.Email, out var address)
            || address.Address != user.Email || !address.Host.Contains('.'))
            throw new ArgumentException("Введіть коректний e-mail.");
        if (!Regex.IsMatch(user.Phone, @"^\+?[0-9]{7,15}$"))
            throw new ArgumentException("Телефон: 7–15 цифр, можна з + на початку.");
    }

    public static void ValidatePassword(string password, string confirmation)
    {
        if (password.Length < 8 || password.Length > 100)
            throw new ArgumentException("Пароль повинен містити 8–100 символів.");
        if (password != confirmation)
            throw new ArgumentException("Пароль і підтвердження не збігаються.");
    }

    public static BankCard CreateCard(int userId, string number, string holder, int month, int year)
    {
        // Keep only the last four digits after validating the supplied number.
        string digits = number.Replace(" ", "").Replace("-", "");
        if (!Regex.IsMatch(digits, @"^[0-9]{16}$") || !IsLuhnValid(digits))
            throw new ArgumentException("Введіть коректний 16-значний тестовий номер картки.");
        string brand = digits.StartsWith('4') ? "Visa"
            : digits.StartsWith('5') ? "Mastercard" : "Other";
        var card = new BankCard
        {
            UserId = userId,
            CardHolder = holder.Trim(),
            Brand = brand,
            LastFourDigits = digits[^4..],
            ExpirationMonth = month,
            ExpirationYear = year
        };
        ValidateCard(card);
        return card;
    }

    public static void ValidateCard(BankCard card)
    {
        Required(card.CardHolder, 100, "Ім’я власника картки");
        if (!Regex.IsMatch(card.LastFourDigits, @"^[0-9]{4}$"))
            throw new ArgumentException("Останні чотири цифри картки некоректні.");
        if (card.Brand != "Visa" && card.Brand != "Mastercard" && card.Brand != "Other")
            throw new ArgumentException("Тип картки: Visa, Mastercard або Other.");
        var today = DateTime.UtcNow;
        if (card.ExpirationMonth < 1 || card.ExpirationMonth > 12
            || card.ExpirationYear < today.Year || card.ExpirationYear > 2100
            || (card.ExpirationYear == today.Year && card.ExpirationMonth < today.Month))
            throw new ArgumentException("Картка прострочена або термін дії некоректний.");
    }

    public static void Required(string value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
            throw new ArgumentException($"{name}: обов’язкове поле, максимум {maxLength} символів.");
    }

    private static bool IsLuhnValid(string number)
    {
        int sum = 0;
        bool doubleDigit = false;
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';
            if (doubleDigit)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
            doubleDigit = !doubleDigit;
        }
        return sum % 10 == 0;
    }
}
