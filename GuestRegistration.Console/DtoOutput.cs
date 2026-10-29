using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp;

internal static class DtoOutput
{
    public static void PrintUser(UserDto? user) => Console.WriteLine(user == null ? "Запис не знайдено."
        : $"ID={user.Id} | {user.Login} | {user.FirstName} {user.LastName} | {user.Gender} | {user.Email} | {user.Phone} | {user.Address} | RoleCode={user.RoleCode} | PasswordHash={user.PasswordHash} | RecoveryWordHash={user.RecoveryWordHash} | CreatedAtUtc={user.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC" + Audit(user));
    public static void PrintCard(BankCardDto? card) => Console.WriteLine(card == null ? "Запис не знайдено."
        : $"ID={card.Id} | UserId={card.UserId} | {card.Brand} ****{card.LastFourDigits} | {card.CardHolder} | {card.ExpirationMonth:00}/{card.ExpirationYear}" + Audit(card));
    public static void PrintAttempt(LoginAttemptDto? attempt) => Console.WriteLine(attempt == null ? "Запис не знайдено."
        : $"ID={attempt.Id} | UserId={attempt.UserId?.ToString() ?? "NULL"} | {attempt.LoginEntered} | {attempt.AttemptedAtUtc:yyyy-MM-dd HH:mm:ss} UTC | Success={attempt.IsSuccessful}" + Audit(attempt));

    private static string Audit(EntityDto dto) => $" | RowInsertTime={dto.RowInsertTime:O} | UpdateTime={dto.UpdateTime:O}";
}
