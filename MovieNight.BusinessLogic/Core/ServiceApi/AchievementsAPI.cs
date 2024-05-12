using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using Newtonsoft.Json;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class AchievementsAPI
    {
        #region Common Elements
        private static readonly string BasePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty)?
            .Parent?.Parent?.FullName;
        private readonly string _jsonPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName ?? string.Empty, @"MovieNight.BusinessLogic\DBModel\Seed\AchievementInfo.json");
        private IMapper _mapper;
        private string JsonP { get; set; }
        private List<AchievementDbTable> AchievementsData { get; set; }


        protected AchievementsAPI()
        {
            JsonP = File.ReadAllText(_jsonPath);
            AchievementsData = JsonConvert.DeserializeObject<List<AchievementDbTable>>(JsonP);
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AchievementDbTable, AchievementE>();
                cfg.CreateMap<AchievementE, AchievementDbTable>();

            });

            _mapper = config.CreateMapper();
            
        }
        
        

        
       

        #endregion
        
        
        #region Сonditions of achievement

        private async Task<AchievementE> RegistrationAnalisAchDb(int? userId)
        {
            using (var user = new UserContext())
            {
                var exist = user.UsersT.Any(u => u.Id == userId);

                if (exist)
                {
                    
                    var achievementData = AchievementsData.FirstOrDefault(a => a.AchievementType == AchievementType.Registration); 
                    if (achievementData != null)
                    {
                        achievementData.Unlocked = true;

                        user.AchievementDb.Add(achievementData);
                        await user.SaveChangesAsync();

                        if (userId != null)
                        {
                            var userAchievement = new UserAchievementDbTable
                            {
                                UserId = userId.Value,
                                AchievementId = achievementData.Id
                            };

                            // Добавьте его в контекст данных
                            user.UserAchievementDb.Add(userAchievement);
                            await user.SaveChangesAsync();
                            return  _mapper.Map<AchievementE>(achievementData);
                        }
                    }

                    return null;
                }
            }

            return null;
        }
        

        #endregion


        #region verification

        protected async Task<AchievementE> AchievementСheckDb((int? userId,AchievementType achType) valueTuple)
        {
            try
            {
                switch (valueTuple.achType)
                {
                    case AchievementType.Registration:
                    {
                        using (var userA = new UserContext())
                        {
                            var verif = userA.AchievementDb.FirstOrDefault(u => u.Id == valueTuple.userId);
                            if (verif != null)
                            {
                                if (!verif.Unlocked)
                                {
                                   return await RegistrationAnalisAchDb(valueTuple.userId);
                                   
                                }
                            }
                            else
                            {
                                return await RegistrationAnalisAchDb(valueTuple.userId);
                            }
                        }

                        break;
                    }case AchievementType.CompleteProfile:{
                    
                    
                        break;
                    }
                
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }


            return null;
        }

        #endregion
    }
}