using System.Text.RegularExpressions;

namespace MovieNight.Helpers.Different
{
    public class ValidationStrD
    {
        public static bool IsEmail(string strVal)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(strVal);
                
        }
    }
}