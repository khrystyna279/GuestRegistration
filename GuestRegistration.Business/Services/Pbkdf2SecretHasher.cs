using System.Globalization;
using System.Security.Cryptography;
using GuestRegistration.Business.Interfaces;

namespace GuestRegistration.Business.Services;

public sealed class Pbkdf2SecretHasher : ISecretHasher
{
    private const int Iterations = 600_000;

    public string Hash(string value)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string value, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;
        try
        {
            string saltText, hashText;
            int iterations;
            if (storedHash.StartsWith("pbkdf2-sha256$", StringComparison.Ordinal))
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out iterations)
                    || iterations < 100_000 || iterations > 2_000_000) return false;
                saltText = parts[2]; hashText = parts[3];
            }
            else
            {
                // Read the initial SQL seed format without changing existing credentials.
                var parts = storedHash.Split('.');
                if (parts.Length != 2) return false;
                saltText = parts[0]; hashText = parts[1]; iterations = 100_000;
            }
            byte[] salt = Convert.FromBase64String(saltText);
            byte[] expected = Convert.FromBase64String(hashText);
            if (salt.Length != 16 || expected.Length != 32) return false;
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(value, salt, iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
        catch (ArgumentException) { return false; }
    }
}
