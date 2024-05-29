using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Domain.Entities.PersonalP;
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

          
        #region verification for achievements
        private bool IsProfileComplete(PEdBdTable profile)
        {
            return !string.IsNullOrWhiteSpace(profile.FirstName) &&
                   !string.IsNullOrWhiteSpace(profile.LastName) &&
                   !string.IsNullOrWhiteSpace(profile.AboutMe) &&
                   !string.IsNullOrWhiteSpace(profile.Gender) &&
                   !string.IsNullOrWhiteSpace(profile.Avatar) &&
                   profile.DataBirth != default &&
                   !string.IsNullOrWhiteSpace(profile.PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(profile.Country) &&
                   !string.IsNullOrWhiteSpace(profile.Quote) &&
                   (!string.IsNullOrWhiteSpace(profile.Facebook) ||
                    !string.IsNullOrWhiteSpace(profile.Twitter) ||
                    !string.IsNullOrWhiteSpace(profile.Instagram) ||
                    !string.IsNullOrWhiteSpace(profile.GitHab));
        }
        private int CountCompletedFields(PEdBdTable profile)
        {
            int count = 0;

            if (!string.IsNullOrWhiteSpace(profile.FirstName)) count++;
            if (!string.IsNullOrWhiteSpace(profile.LastName)) count++;
            if (!string.IsNullOrWhiteSpace(profile.AboutMe)) count++;
            if (!string.IsNullOrWhiteSpace(profile.Gender)) count++;
            if (!string.IsNullOrWhiteSpace(profile.Avatar)) count++;
            if (profile.DataBirth != default) count++;
            if (!string.IsNullOrWhiteSpace(profile.PhoneNumber)) count++;
            if (!string.IsNullOrWhiteSpace(profile.Country)) count++;
            if (!string.IsNullOrWhiteSpace(profile.Quote)) count++;

            if (!string.IsNullOrWhiteSpace(profile.Facebook) ||
                !string.IsNullOrWhiteSpace(profile.Twitter) ||
                !string.IsNullOrWhiteSpace(profile.Instagram) ||
                !string.IsNullOrWhiteSpace(profile.GitHab)) count++;

            return count;
        }
        
        #endregion
        
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
                        achievementData.SuccessСount++;
                        user.AchievementDb.Add(achievementData);
                        await user.SaveChangesAsync();

                        if (userId != null)
                        {
                            var userAchievement = new UserAchievementDbTable
                            {
                                UserId = userId.Value,
                                AchievementId = achievementData.Id
                            };

                            
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
        
        
        private async Task<AchievementE> FullProfileAnalisAchDb(int? userId)
        {
            using (var user = new UserContext())
            {
                var exist = user.UsersT.Any(u => u.Id == userId);

                if (exist)
                {
                    var profile = await user.PEdBdTables.FirstOrDefaultAsync(p => p.UserDbTableId == userId);
                    if (profile != null)
                    {
                        var completedFieldsCount = CountCompletedFields(profile);
                        var userAchievement = user.UserAchievementDb
                            .Include(ua => ua.Achievement)
                            .FirstOrDefault(ua => ua.UserId == userId.Value && ua.Achievement.AchievementType == AchievementType.CompleteProfile);
                        if (userAchievement != null)
                        {
                            userAchievement.Achievement.SuccessСount = completedFieldsCount;
                            if (completedFieldsCount == userAchievement.Achievement.ProgressСount)
                            {
                                userAchievement.Achievement.Unlocked = true;
                            }
                            await user.SaveChangesAsync();
                            return _mapper.Map<AchievementE>(userAchievement);
                        }
                        else
                        {
                            var achievementData = AchievementsData.FirstOrDefault(a => a.AchievementType == AchievementType.CompleteProfile);
                            if (achievementData != null)
                            {
                                achievementData.SuccessСount = completedFieldsCount;
                                if (completedFieldsCount == achievementData.ProgressСount)
                                {
                                    achievementData.Unlocked = true;
                                }
                                user.AchievementDb.Add(achievementData);


                                if (userId != null)
                                    userAchievement = new UserAchievementDbTable
                                    {
                                        UserId = userId.Value,
                                        AchievementId = achievementData.Id
                                    };
                                if (userAchievement != null) user.UserAchievementDb.Add(userAchievement);
                                await user.SaveChangesAsync();
                                
                            
                                return _mapper.Map<AchievementE>(achievementData);
                            }
                        }
                        
                        
                    }
                }
            }

            return null;
        }

        private async Task<AchievementE> FirstMovieAnalisAchDb(int? userId)
        {
            using (var user = new UserContext())
            {
                var exist = user.UsersT.Any(u => u.Id == userId);

                if (exist)
                {
                   
                    var hasWatchedMovie = user.ViewList.Any(v => v.UserId == userId);

                    if (hasWatchedMovie)
                    {
                        var achievementData = AchievementsData.FirstOrDefault(a => a.AchievementType == AchievementType.FirstMovie);

                        if (achievementData != null)
                        {
                            achievementData.Unlocked = true;
                            achievementData.SuccessСount++;
                            user.AchievementDb.Add(achievementData);
                            await user.SaveChangesAsync();

                            if (userId != null)
                            {
                                var userAchievement = new UserAchievementDbTable
                                {
                                    UserId = userId.Value,
                                    AchievementId = achievementData.Id
                                };

                                user.UserAchievementDb.Add(userAchievement);
                                await user.SaveChangesAsync();
                                return _mapper.Map<AchievementE>(achievementData);
                            }
                        }

                        return null;
                    }
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
                            var verif = userA.UserAchievementDb.Include(ua => ua.Achievement)
                                .FirstOrDefault(u => u.UserId == valueTuple.userId  && u.Achievement.AchievementType == AchievementType.Registration)
                                ?.Achievement;
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
                    }
                    case AchievementType.CompleteProfile:{

                        using (var userA = new UserContext())
                        {
                            var verif = userA.UserAchievementDb.Include(ua => ua.Achievement)
                                .FirstOrDefault(u => u.UserId == valueTuple.userId  && u.Achievement.AchievementType == AchievementType.CompleteProfile)
                                ?.Achievement;
                            if (verif != null)
                            {
                                if (!verif.Unlocked)
                                {
                                    return await FullProfileAnalisAchDb(valueTuple.userId);
                                }
                            }
                            else
                            {
                                return await FullProfileAnalisAchDb(valueTuple.userId);
                            }
                        }
                    
                        break;
                    }
                    case AchievementType.FirstMovie:
                    {
                        using (var userA = new UserContext())
                        {
                            var verif = userA.UserAchievementDb.Include(ua => ua.Achievement)
                                .FirstOrDefault(u => u.UserId == valueTuple.userId  && u.Achievement.AchievementType == AchievementType.FirstMovie)?.Achievement;
                            if (verif != null)
                            {
                                if (!verif.Unlocked)
                                {
                                    return await FirstMovieAnalisAchDb(valueTuple.userId);
                                }
                            }
                            else
                            {
                                return await FirstMovieAnalisAchDb(valueTuple.userId);
                            }
                        }
                    
                        break;
                    }
                    case AchievementType.FirstFriend:
                    {
                        
                        break;
                    }
                    case AchievementType.SocialButterfly:
                    {
                        
                        break;
                    }
                    case AchievementType.HorrorEnthusiast:
                    {
                        
                        break;
                    }
                    case AchievementType.MovieBuff:
                    {
                        
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
        
        
        #region Get

        protected List<AchievementE> GetAchievementsDb(int? userId)
        {
            try
            {
                using (var user = new UserContext())
                {
                    var achievements = user.UserAchievementDb.Where(a => a.UserId == userId).Include(a => a.Achievement).ToList();
                    if(achievements.Count!=0) {
                        var achievementList = new List<AchievementE>();
                        if (achievementList == null) throw new ArgumentNullException(nameof(achievementList));
                        foreach (var achievement in achievements)
                        {
                            achievementList.Add(_mapper.Map<AchievementE>(achievement.Achievement));
                        }

                        return achievementList;
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