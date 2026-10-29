using GuestRegistration.Business.Models;

namespace GuestRegistration.Business.Services;

public sealed class UserSession
{
    public AuthenticatedUser? Current { get; private set; }
    internal void Start(AuthenticatedUser user) => Current = user;
    internal void Clear() => Current = null;
}
