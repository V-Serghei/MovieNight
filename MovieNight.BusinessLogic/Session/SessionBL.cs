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
using MovieNight.Domain.Entities.UserId.ResultE;

namespace MovieNight.BusinessLogic.Session
{

    public class SessionBL: UserApi, ISession 
    {

        //  - > login
        public async Task<UserVerification> UserVerification(LogInData logInData)
        {
            return await GetUserVerification(logInData);
        }


        //  - > registration
        public async Task<UserRegister> UserAdd(RegData rData)
        {
            return await AddNewUserSuccess(rData);
        }

        public bool UserСreation(RegData rData)
        {
            return UserAdding(rData);
        }

        //  - > user read
        public UserE GetUserData(int? userId)
        {
            return GetUserDataFromDatabase(userId);
        }

        public int? GetUserIdFromSession()
        {
            return GetUserId();
        }

        public PersonalProfileM GetPersonalProfileM(int? userId)
        {
            return GetPersonalProfileDatabase(userId);
        }

        //  - > change of user data
        public SuccessOfTheActivity EdProfInfo(ProfEditingE editing)
        {
            return EditingProfileData(editing);
        }

        public HttpCookie GenCookie(LogInData userD)
        {
            return Cookie(userD);
        }

        public LogInData GetUserByCookie(string apiCookieValue, string agent)
        {
            return UserCookie(apiCookieValue,agent);
        }

        public int? GetIdCurrUser(string userName)
        {
            return GetIdCurrUserDb(userName);
        }
        
       public bool DelSessionCurrUser(string userCurrent){
            return DelSessionCurrUserDb(userCurrent);
        }

        public HttpCookie GenCookieLongTime(LogInData verificationLogInData)
        {
            return CookieLongTime(verificationLogInData);
        }


        public void CleanupExpiredSessionRange()
        {
            CleanupExpiredSessions();
        }

        public async Task<ReserPasswordResult> UserResetPassword(string modelEmail, string modelNewPassword)
        {
            return await UserResetPasswordDb(modelEmail, modelNewPassword);
        }

        //  - > user data update
        //...

        //  - > 
        //...




    }
}
