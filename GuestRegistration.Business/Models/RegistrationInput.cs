namespace GuestRegistration.Business.Models;

public sealed record RegistrationInput
{
    public string Login { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Password { get; init; } = "";
    public string ConfirmPassword { get; init; } = "";
    public string RecoveryWord { get; init; } = "";
    public string Gender { get; init; } = "";
    public string Address { get; init; } = "";
    public string Email { get; init; } = "";
    public string Phone { get; init; } = "";
    public string CardNumber { get; init; } = "";
    public string CardHolder { get; init; } = "";
    public string ExpirationMonth { get; init; } = "";
    public string ExpirationYear { get; init; } = "";
}
