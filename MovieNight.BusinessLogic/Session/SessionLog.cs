using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MovieNight.BusinessLogic.Core;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Session
{

    public class SessionLog: UserApi, ISession 
    {
         public UserVerification UserVerification(LogInData logInData)
         {
            return GetUserVerification(logInData);
         }

         public UserRegister UserAdd(RegData rData)
         {
             return AddNewUserSuccess(rData);
         }

         public bool UserСreation(RegData rData)
         {
             return UserAdding(rData);
         }

         public void SetUserSession(int userId)
         {
             HttpContext.Current.Session["UserId"] = userId;
             HttpContext.Current.Session["UserName"] = GetUserData(userId).Name;
         }

         public UserE GetUserData(int? userId)
         {
             return GetUserDataFromDatabase(userId);
         }

        public int? GetUserIdFromSession()
         {
             if (HttpContext.Current?.Session != null)
             {
                 return (int?)HttpContext.Current.Session["UserId"];
             }
             return null;
         }

        public PersonalProfileM GetPersonalProfileM(int? userId)
        {
            return GetPersonalProfileDatabase(userId);
        }




    }
}
