namespace GuestRegistration.Business.Interfaces;

public interface ISecretHasher
{
    string Hash(string value);
    bool Verify(string value, string storedHash);
}
