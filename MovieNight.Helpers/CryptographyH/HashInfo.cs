using System;
using System.Security.Cryptography;
using System.Text;

namespace MovieNight.Helpers.CryptographyH
{
    public static class HashInfo
    {
        public static string HashInf(string password, string salt)
        {
            var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);

            var hashedPassword = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(24));
            return hashedPassword;
        }
        public static bool VerifyInf(string providedPassword, string storedHash, string salt)
        {
            var newHash = HashInf(providedPassword, salt);
            return newHash == storedHash;
        }
    }
}