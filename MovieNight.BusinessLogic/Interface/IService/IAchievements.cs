using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IAchievements
    {
        #region Verification

        Task<AchievementE> AchievementСheck((int? userId, AchievementType achType) valueTuple);
        

        #endregion

        
        #region Get
        
        List<AchievementE> GetAchievements(int? userId);

        
        #endregion
    }
}