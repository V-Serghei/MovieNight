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


        //  - > user data update
        //...

        //  - > 
        //...




    }
}
