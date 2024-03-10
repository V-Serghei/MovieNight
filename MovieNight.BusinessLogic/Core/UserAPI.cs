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

        public UserRegister AddNewUserSuccess(RegData rData)
        {
            var exUserAip = new UserRegister();
            //check the database for such a user
            if (rData.FullName != "" && rData.Password != "" && rData.Email != "")
            {
                exUserAip.SuccessUniq = true;
            }
            else exUserAip.SuccessUniq = false;
            return exUserAip;
        }

        public bool userAdding(RegData rData)
        {
            //add user to database

            if (rData.FullName != null && rData.Password != null && rData.Email != null)
            {
                return true;
            }
            return false;
        }
    }
}
