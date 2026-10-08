namespace GuestRegistration.DAL.Services;

public class RegistrationRequest
{
    public string Login { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string RecoveryWord { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string CardNumber { get; set; } = "";
    public string CardHolder { get; set; } = "";
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
}
