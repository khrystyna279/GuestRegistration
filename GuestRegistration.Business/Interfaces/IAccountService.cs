using GuestRegistration.Business.Models;

namespace GuestRegistration.Business.Interfaces;

public interface IAccountService
{
    AuthenticatedUser? CurrentUser { get; }
    AccountResult Register(RegistrationInput input);
    AccountResult Login(LoginInput input);
    AccountResult RecoverPassword(RecoveryInput input);
    void Logout();
}

public interface IProfileService
{
    UserProfile GetCurrentProfile();
}
