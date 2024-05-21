using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class AchievementsService:AchievementsAPI,IAchievements
    {
        #region Verification
        public async Task<AchievementE> AchievementСheck((int? userId, AchievementType achType) valueTuple)
        {
            return await AchievementСheckDb(valueTuple);
        }

      

        #endregion
        
        #region Get
        
        public List<AchievementE> GetAchievements(int? userId)
        {
            return GetAchievementsDb(userId);
        }
        #endregion
        
    }
}