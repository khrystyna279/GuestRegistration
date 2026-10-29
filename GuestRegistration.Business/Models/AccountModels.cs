namespace GuestRegistration.Business.Models;

public sealed record LoginInput(string Login, string Password);
public sealed record RecoveryInput(string Login, string RecoveryWord, string Password, string ConfirmPassword);
public sealed record AuthenticatedUser(int Id, string Login, string DisplayName, bool HasGuestAccess);
public sealed record CardSummary(string Holder, string Brand, string LastFourDigits, int Month, int Year);
public sealed record UserProfile(string Login, string FirstName, string LastName, string Gender,
    string Address, string Email, string Phone, IReadOnlyList<CardSummary> Cards);

public sealed record AccountResult(bool IsSuccess, IReadOnlyDictionary<string, string> Errors, AuthenticatedUser? User = null)
{
    public static AccountResult Success(AuthenticatedUser? user = null) => new(true, new Dictionary<string, string>(), user);
    public static AccountResult Failure(string message, string field = "") => new(false, new Dictionary<string, string> { [field] = message });
    public static AccountResult Invalid(IReadOnlyDictionary<string, string> errors) => new(false, errors);
}
