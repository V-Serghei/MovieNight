using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Session
{

    public class SessionLog: UserApi, ISession 
    {
         public UserVerification UserVerification(LogInData logInData)
         {
            return GetUserVerification(logInData);
         }

    }
}
