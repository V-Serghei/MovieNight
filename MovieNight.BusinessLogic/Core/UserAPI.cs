using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Core
{
    public class UserApi
    {
        public UserVerification GetUserVerification(LogInData logInData)
        {
            var exUserAip = new UserVerification();
            //database search logic

            if(logInData.Username == "vistovschii@gmail.com" && logInData.Password == "1111") 
            {
                exUserAip.IsVerified = true;
                exUserAip.LogInData = new LogInData
                {
                    Password = logInData.Password,
                    Username = logInData.Username,
                    RememberMe = logInData.RememberMe
                };
                
            }else
            {
                exUserAip.IsVerified = false;
            }
            return exUserAip;
        }
    }
}
