namespace GuestRegistration.DTO;

public sealed class BankCardDto : EntityDto
{
    public int UserId { get; set; }
    public string CardHolder { get; set; } = "";
    public string Brand { get; set; } = "";
    public string LastFourDigits { get; set; } = "";
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
}
