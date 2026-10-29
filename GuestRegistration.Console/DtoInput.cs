using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp;

internal static class DtoInput
{
    public static UserDto ReadUser() => new()
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
        CreatedAtUtc = ConsoleInput.ReadUtcDateTime("CreatedAtUtc"),
        RoleCode = ConsoleInput.Read("RoleCode (Guest / Restricted)")
    };

    public static BankCardDto ReadCard() => new()
    {
        UserId = ConsoleInput.ReadInt("UserId"),
        CardHolder = ConsoleInput.Read("CardHolder"),
        Brand = ConsoleInput.Read("Brand (Visa / Mastercard / Other)"),
        LastFourDigits = ConsoleInput.Read("LastFourDigits (4 цифри)"),
        ExpirationMonth = ConsoleInput.ReadInt("ExpirationMonth (1–12)"),
        ExpirationYear = ConsoleInput.ReadInt("ExpirationYear (2020–2100)")
    };

    public static LoginAttemptDto ReadAttempt()
    {
        int? userId = ConsoleInput.ReadOptionalId("UserId");
        string login = ConsoleInput.Read("LoginEntered");
        DateTime attemptedAtUtc = ConsoleInput.ReadUtcDateTime("AttemptedAtUtc");
        return new LoginAttemptDto
        {
            UserId = userId,
            LoginEntered = login,
            AttemptedAtUtc = attemptedAtUtc,
            IsSuccessful = ConsoleInput.ReadBool("IsSuccessful")
        };
    }
}
