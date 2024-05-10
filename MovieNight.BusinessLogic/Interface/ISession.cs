using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Interface
{
    public interface ISession
    {
        Task<UserVerification> UserVerification(LogInData logInData);
        Task<UserRegister> UserAdd(RegData rData);
        bool UserСreation(RegData rData);
        int? GetUserIdFromSession();
        UserE GetUserData(int? userId);
        PersonalProfileM GetPersonalProfileM(int? userId);
        SuccessOfTheActivity EdProfInfo(ProfEditingE editing);
        HttpCookie GenCookie(LogInData userD);
        LogInData GetUserByCookie(string apiCookieValue,string agent);

        int? GetIdCurrUser(string userName);
        bool DelSessionCurrUser(string userCurrent);
    }
}
