namespace GuestRegistration.DTO;

public sealed class LoginAttemptDto : EntityDto
{
    public int? UserId { get; set; }
    public string LoginEntered { get; set; } = "";
    public DateTime AttemptedAtUtc { get; set; }
    public bool IsSuccessful { get; set; }
}
