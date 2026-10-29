namespace GuestRegistration.DTO;

public sealed class UserDto : EntityDto
{
    public string RoleCode { get; set; } = "Guest";

    public string Login { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string RecoveryWordHash { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}
