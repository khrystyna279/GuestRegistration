namespace GuestRegistration.DAL.Interfaces;

public interface IDataSession : IDisposable
{
    IUserRepository Users { get; }
    IBankCardRepository Cards { get; }
    ILoginAttemptRepository Attempts { get; }
    void Commit();
}
