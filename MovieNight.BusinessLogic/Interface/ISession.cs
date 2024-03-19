using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Interface
{
    public interface ISession
    {
        UserVerification UserVerification(LogInData logInData);
        UserRegister UserAdd(RegData rData);
        bool UserСreation(RegData rData);
        void SetUserSession(int userId);
        int? GetUserIdFromSession();
        UserE GetUserData(int? userId);
        PersonalProfileM GetPersonalProfileM(int? userId);
    }
}
