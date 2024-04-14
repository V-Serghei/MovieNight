using System;
using System.Security.Cryptography;

namespace MovieNight.Helpers.CryptographyH
{
    public static class Salt
    {
        public static string GetRandSalt()
        {
            var rng = new RNGCryptoServiceProvider();
            var salt = new byte[16];
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
    }
}