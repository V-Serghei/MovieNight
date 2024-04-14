using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MovieNight.Helpers.CryptographyH;

namespace MovieNight.Helpers.CookieH
{
    public static class GenCookie
    {
        
        
        public static string Create(string value,string salt, string key)
        {
            return EncryptStringAes(value, salt,key);
        }

        public static string Validate(string value,string salt, string key)
        {
            return DecryptStringAes(value, salt,key);
        }
        
        
        

       
        private static string EncryptStringAes(string plainText,string salt, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException(nameof(sharedSecret));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            string outStr;                       
            RijndaelManaged aesAlg = null;             

            try
            {
                var key = new Rfc2898DeriveBytes(sharedSecret, Encoding.ASCII.GetBytes(salt));

                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                aesAlg?.Clear();
            }

            return outStr;
        }

        
        private static string DecryptStringAes(string cipherText, string salt, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException(nameof(sharedSecret));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            string plaintext = null;
            RijndaelManaged aesAlg = null;

            try
            {
                var key = new Rfc2898DeriveBytes(sharedSecret, Encoding.ASCII.GetBytes(salt));

                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                var bytes = Convert.FromBase64String(cipherText);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    ICryptoTransform decrypted = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decrypted, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }
        
    }
}