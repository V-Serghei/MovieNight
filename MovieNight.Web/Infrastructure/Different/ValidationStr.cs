using System.Text.RegularExpressions;

namespace MovieNight.Web.Infrastructure.Different
{
    public class ValidationStr
    {
        public static bool IsEmail(string strVal)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(strVal);
                
        }
    }
}