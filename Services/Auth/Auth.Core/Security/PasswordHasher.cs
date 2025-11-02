using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Auth.Core.Security;

public static class PasswordHasher
{
    public static (string hash, string salt) Hash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password, saltBytes, KeyDerivationPrf.HMACSHA256, 100_000, 32));
        return (hash, salt);
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var computed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password, saltBytes, KeyDerivationPrf.HMACSHA256, 100_000, 32));
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hash), Convert.FromBase64String(computed));
    }
}