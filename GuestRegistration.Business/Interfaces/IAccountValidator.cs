using GuestRegistration.Business.Models;

namespace GuestRegistration.Business.Interfaces;

public interface IAccountValidator
{
    IReadOnlyDictionary<string, string> Registration(RegistrationInput input);
    IReadOnlyDictionary<string, string> Login(LoginInput input);
    IReadOnlyDictionary<string, string> Recovery(RecoveryInput input);
}
