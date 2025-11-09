using System.Text;

namespace Auth.Core.Security;

using System.Text;


public static class HashToken
{
    public static string HashTokenEncoding(string raw)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
}
