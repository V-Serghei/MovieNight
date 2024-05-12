using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class AchievementsService:AchievementsAPI,IAchievements
    {
        public async Task<AchievementE> AchievementСheck((int? userId, AchievementType achType) valueTuple)
        {
          return await AchievementСheckDb(valueTuple);
        }
    }
}