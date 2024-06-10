using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Helpers.CookieH;
using MovieNight.Helpers.CryptographyH;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.UserId.ResultE;

namespace MovieNight.BusinessLogic.Core
{
    public class UserApi
    {
        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //  - > password-related methods     
        //  |  |  |  |  |  |  |  |  |  |  |  |
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/
        // public string HashPassword(string password, string salt)
        // {
        //     var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);
        //
        //     var hashedPassword = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(24));
        //     return hashedPassword;
        // }
        //
        // public bool VerifyPassword(string providedPassword, string storedHash, string salt)
        // {
        //     var newHash = HashPassword(providedPassword, salt);
        //     return newHash == storedHash;
        // }
        //
        // private string GetRandSalt()
        // {
        //     var rng = new RNGCryptoServiceProvider();
        //     var salt = new byte[16];
        //     rng.GetBytes(salt);
        //     return Convert.ToBase64String(salt);
        //
        // }

        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //           - > login  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/
        private IMapper _mapper;
        
        private bool IsValid(LogInData rData)
        {
            if (string.IsNullOrEmpty(rData.Password) || (string.IsNullOrEmpty(rData.Email) && string.IsNullOrEmpty(rData.Username)))
                return false;

            return rData.Password.Length >= 10;
        }
        protected async Task<UserVerification> GetUserVerification(LogInData logInData)
        {
            var userL = new UserVerification();

            if (!IsValid(logInData))
            {
                userL.StatusMsg = "Invalid data";
                userL.IsVerified = false;
                return userL;
            }

            using (var db = new UserContext())
            {
                UserDbTable userExists;
                if (logInData.Email != null)
                {
                     userExists = await db.UsersT.FirstOrDefaultAsync(u => u.Email == logInData.Email);
                }
                else
                {
                     userExists = await db.UsersT.FirstOrDefaultAsync(u => u.UserName == logInData.Username);
                }
                if (userExists == null)
                {
                        userL.StatusMsg = "You have entered unrecorded data";
                        userL.IsVerified = false;
                    return userL;
                }else if (!HashPassword.VerifyPassword(logInData.Password, userExists.Password, userExists.Salt))
                {
                    userL.StatusMsg = "You entered the wrong password";
                    userL.IsVerified = false;
                    return userL;
                }
                else
                {
                    userL.StatusMsg = "Success";
                    userL.IsVerified = true;
                    userL.UserId = userExists.Id;
                    userL.LogInData = logInData;
                    userL.LogInData.Username = userExists.UserName;
                    userL.LogInData.Email = userExists.Email;
                    userL.LogInData.Role = userExists.Role;
                    userL.LogInData.Id = userExists.Id;
                    
                    var userD = db.PEdBdTables.FirstOrDefault(u => u.UserDbTableId == userExists.Id);
                    if(userD?.Avatar != null) userL.LogInData.Avatar = userD.Avatar;
                    HttpContext.Current.Session["UserId"] = userExists.Id;
                    HttpContext.Current.Session["UserName"] = userExists.UserName;
                    return userL;
                }
            }

        }
        /// <param name="rData"></param>
        /// <returns></returns>

        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //       - > registration  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/
        private bool IsValid(RegData rData)
        {
            if (string.IsNullOrEmpty(rData.UserName) || string.IsNullOrEmpty(rData.Password) || string.IsNullOrEmpty(rData.Email))
                return false;
            
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(rData.Email))
                return false;
            
            if (rData.Password.Length < 10)
                return false;

            return true;
        }
        protected async Task<UserRegister> AddNewUserSuccess(RegData rData)
        {
            var userRegister = new UserRegister();

            if (!IsValid(rData))
            {
                userRegister.StatusMsg = "Invalid data";
                userRegister.SuccessUniq = false;
                return userRegister;
            }
            
            using (var db = new UserContext())
            {
                var userExists = await db.UsersT.FirstOrDefaultAsync(u => u.UserName == rData.UserName || u.Email == rData.Email);

                if (userExists != null)
                {
                    userRegister.StatusMsg = userExists.UserName == rData.UserName ? "Username already taken" : "Email already in use";

                    return userRegister;
                }
            }
            
            userRegister.SuccessUniq = true;
            userRegister.StatusMsg = "Success";
            userRegister.CurUser = new LogInData
            {
                Username = rData.UserName,
                Email = rData.Email,
                Password = rData.Password,
                Role = LevelOfAccess.User
            };
            
            
           
            return userRegister;
        }
        protected static bool UserAdding(RegData rData)
        {
            var user = new UserDbTable()
            {
                UserName = rData.UserName,
                Email = rData.Email,
                LastLoginDate = rData.RegDateTime,
                LastIp = rData.Ip,
                Role = LevelOfAccess.User,
                Checkbox = rData.Checkbox,
                Salt = Salt.GetRandSalt()
                

            };
            user.Password = HashPassword.HashPass(rData.Password, user.Salt);
            
            using (var db = new UserContext())
            {
                try
                {

                    db.UsersT.Add(user);
                    db.SaveChanges();
                    HttpContext.Current.Session["UserId"] = user.Id;
                    HttpContext.Current.Session["UserName"] = user.UserName;
                    return true;
                }
                catch (DbEntityValidationException  ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Console.WriteLine("Сущность: {0}, Свойство: {1}, Ошибка: {2}",
                                validationErrors.Entry.Entity.GetType().Name,
                                validationError.PropertyName,
                                validationError.ErrorMessage);
                        }
                    }
                    return false;
                }
            }
        }



        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //        - > user read  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/

        protected UserE GetUserDataFromDatabase(int? userId)
        {


            var userE = new UserE();
            if (userId == null) return userE;

            using (var db = new UserContext())
            {
                var userExists = db.UsersT.FirstOrDefault(u => u.Id == userId);

                if (userExists != null)
                {
                    userE.Email = userExists.Email;
                    userE.Password = userExists.Password;
                    userE.Id = userExists.Id;
                    userE.Username = userExists.UserName;
                    return userE;
                }
            }
        

            return userE;
        }
        protected PersonalProfileM GetPersonalProfileDatabase(int? userId)
        {
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PEdBdTable, PersonalProfileM>()
                    .ForMember(dist => dist.BUserE,
                        src => src.Ignore());

            });

            _mapper = config.CreateMapper();

            using (var userProfData = new UserContext())
            {
                try
                {
                    
                    var userProfCurrent = userProfData.PEdBdTables.FirstOrDefault(u => u.User.Id == userId);
                    var userP = _mapper.Map<PersonalProfileM>(userProfCurrent);
                    if (userP != null) return userP;
                    var userDef = GetUserDataFromDatabase(userId);
                    
                   
                    return new PersonalProfileM
                    {
                        BUserE = userDef
                    };
                }
                catch (Exception ex)
                {
                    var userDef = GetUserDataFromDatabase(userId);
                    return new PersonalProfileM
                    {
                        BUserE = userDef
                    };
                }
            }
           
            
            
            
            // PersonalProfileM personalProfileM = new PersonalProfileM
            // {
            //     Avatar = "~/images/users/photo_2023-03-30_21-08-09.jpg",
            //     BUserE = new UserE
            //     {
            //         Email = GetUserDataFromDatabase(userId).Email, 
            //         Username= GetUserDataFromDatabase(userId).Username,
            //     },
            //     Quote = "Movie fan",
            //     AboutMe = "I’m Nelly and I love watching movies. Especially anime. For me, nothing is better than anime. Yes, and I’m also a cool IT girl and designer. And I’m also a master. I can do everything. " +
            //               "Cook, sew, draw, sculpt. I can learn everything. The main thing is to want.",
            //     Location = "Moldova",
            //     ViewingHistory = new List<ViewingHistoryM>(),
            //     ListInThePlans = new List<ListOfFilmsE>()
            //
            // };
            // //Get out of the movie database
            // for (int i = 0; i < 5; i++)
            // {
            //     personalProfileM.ListInThePlans.Add(new ListOfFilmsE
            //     {
            //         Date = new TimeD
            //         {
            //             Day = 10+i,
            //             Month = 10,
            //             Year = 2024
            //         },
            //         Name = "The Shawshank Redemption",
            //         NumberOfViews = 1102031331,
            //         Star = 5+i,
            //         Tags = new List<Tag>
            //         {
            //             new Tag
            //             {
            //                 Id = 15+i,
            //                 Name = "drama"
            //             },
            //             new Tag
            //             {
            //             Id = 12+i,
            //             Name = "family"
            //         }
            //         }
            //         
            //     });
            //     
            //
            // }
            // for (int i = 0; i < 5; i++)
            // {
            //     personalProfileM.ViewingHistory.Add(new ViewingHistoryM
            //     {
            //         Description = "",
            //         Id = i,
            //         Title = "Solo Leveling",
            //         Poster = new Poster
            //         {
            //             Id = i,
            //             Name = "Solo Leveling",
            //             Path = "~/images/index2.jpg",
            //         },
            //         Star = i+4,
            //         ViewingTime = DateTime.Now
            //        
            //     });
            // }
            //
            // return personalProfileM;
        }
        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //        - > user editing  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/


        private UserDbTable GetCurrentLoggedInUserDb()
        {
            var userId = GetUserId(); 

            using (var db = new UserContext())
            {
                return db.UsersT.FirstOrDefault(u => u.Id == userId);
            }
        }
        internal int? GetUserId()
        {
            if (HttpContext.Current?.Session != null)
            {
                return (int?)HttpContext.Current.Session["UserId"];
            }
            return null;
        }
        protected static int? GetIdCurrUserDb(string userName)
        {
            using (var db = new UserContext())
            {
                var existingUser = db.UsersT.FirstOrDefault(u => u.UserName == userName);
                return existingUser?.Id;
            }
            
        }
        protected SuccessOfTheActivity EditingProfileData(ProfEditingE editing)
        {
            var result = new SuccessOfTheActivity();
            var currentUser = GetCurrentLoggedInUserDb();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProfEditingE, PEdBdTable>()
                    .ForMember(dest => dest.UserDbTableId,
                        opt => opt.Ignore())
                    .ForMember(dist => dist.Avatar,
                        opt => opt.Ignore());
            });

            var mapper = config.CreateMapper();
            using (var db = new UserContext())
            {
                try
                {
                    var existingUser = db.UsersT.FirstOrDefault(u => u.Id == currentUser.Id);
                    var existingProfile = db.PEdBdTables.FirstOrDefault(u => u.UserDbTableId == existingUser.Id);
                    if (editing.DataBirth == default(DateTime))
                    {
                        editing.DataBirth = new DateTime(1753, 1, 1);
                    }
                    if (existingProfile != null)
                    {
                        mapper.Map(editing, existingProfile);
                        if (existingProfile.Avatar != null && editing.Avatar != null)
                        {
                            existingProfile.Avatar = editing.Avatar;
                        }
                        else if (existingProfile.Avatar == null && editing.Avatar != null)
                        {
                            existingProfile.Avatar = editing.Avatar;
                        }
                        
                        
                        if (existingUser != null)
                        {
                            if (editing.Username != null)
                                    existingUser.UserName = editing.Username;
                            if (editing.Email != null && editing.Email!="Error")
                                existingUser.Email = editing.Email;
                            // if (editing.Password != null && editing.Password!="Error")
                            //     existingUser.Password = HashPassword.HashPass(editing.Password,existingUser.Salt);
                        }
                       
                    }
                    else
                    {
                        var newProfile = new PEdBdTable { User = existingUser };
                        mapper.Map(editing, newProfile);
                        newProfile.Avatar = editing.Avatar;
                        db.PEdBdTables.Add(newProfile);
                    }

                    db.SaveChanges();
                    result.Successes = true;
                    result.Msg = "Successful retention in the database!";
                    return result;
                }
                catch (Exception ex)
                {
                    result.Successes = false;
                    result.Msg = ex.Message;
                    return result;
                }
            }
        }
        internal  HttpCookie Cookie(LogInData userD)
        {
            using (var us = new UserContext())
            {
                var user = (from u in us.UsersT where u.Email == userD.Email select u).FirstOrDefault();

                Debug.Assert(user != null, nameof(user) + " != null");
                var apiCookie = new HttpCookie("X-KEY")
                {
                    Value = GenCookie.Create(userD.Email + userD.Agent, 
                        user.Salt, HashInfo.HashInf(userD.Email + userD.Agent,user.Salt))
                };

                using (var db = new SessionContext())
                {
                    var userCookie = (from e in db.Sessions where e.Email == userD.Email select e).FirstOrDefault();

                    if (userCookie != null)
                    {
                        userCookie.CookieString = apiCookie.Value;
                        userCookie.ExpireTime = DateTime.Now.AddMinutes(60);
                        using (var todo = new SessionContext())
                        {
                            todo.Entry(userCookie).State = EntityState.Modified;
                            todo.SaveChanges();
                        }
                    }
                    else
                    {

                        db.Sessions.Add(new SessionCookie
                        {
                            CookieString = apiCookie.Value,
                            ExpireTime = DateTime.Now.AddMinutes(60),
                            Email = userD.Email,
                            UserName = userD.Username,

                        });

                        db.SaveChanges();
                    }
                }

                return apiCookie;
            }
        }
        internal static LogInData UserCookie(string cookie, string agent)
        {
            SessionCookie session;
            UserDbTable currentUser;

            using (var db = new SessionContext())
            {
                session = db.Sessions.FirstOrDefault(s => s.CookieString == cookie && s.ExpireTime > DateTime.Now);
            }

            if (session == null) return null;
            using (var db = new UserContext())
            {
                var validate = new EmailAddressAttribute();
                currentUser = validate.IsValid(session.Email) ? 
                    db.UsersT.FirstOrDefault(u => u.Email == session.Email) : 
                    db.UsersT.FirstOrDefault(u => u.UserName == session.UserName);
                
            }
            
            if (currentUser == null) return null;
            var apiCookie = new HttpCookie("X-KEY")
            {
                Value = GenCookie.Create(currentUser.Email + agent, 
                    currentUser.Salt, HashInfo.HashInf(currentUser.Email + agent,currentUser.Salt))
            };
            if (apiCookie.Value != cookie) return null;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDbTable, LogInData>();
            });

            var mapper = config.CreateMapper();
            var userLog = mapper.Map<LogInData>(currentUser);
            using (var db = new UserContext())
            {
                var avatar = db.PEdBdTables.FirstOrDefault(u => u.UserDbTableId == currentUser.Id);
                if (avatar != null) userLog.Avatar = avatar.Avatar;
            }

            return userLog;
        }
        protected bool DelSessionCurrUserDb(string userN)
        {
            SessionCookie session;
            UserDbTable currentUser;

            using (var db = new SessionContext())
            {
                session = db.Sessions.FirstOrDefault(s => s.UserName == userN);
                if (session != null)
                {
                    db.Sessions.Remove(session);
                    db.SaveChanges();
                    return true;
                }
            }
            
            return false;
        }
        internal  HttpCookie CookieLongTime(LogInData userD)
        {
            using (var us = new UserContext())
            {
                var user = (from u in us.UsersT where u.Email == userD.Email select u).FirstOrDefault();

                Debug.Assert(user != null, nameof(user) + " != null");
                var apiCookie = new HttpCookie("X-KEY")
                {
                    Value = GenCookie.Create(userD.Email + userD.Agent, 
                        user.Salt, HashInfo.HashInf(userD.Email + userD.Agent,user.Salt))
                };

                using (var db = new SessionContext())
                {
                    var userCookie = (from e in db.Sessions where e.Email == userD.Email select e).FirstOrDefault();

                    if (userCookie != null)
                    {
                        userCookie.CookieString = apiCookie.Value;
                        userCookie.ExpireTime = DateTime.Now.AddDays(20);
                        using (var todo = new SessionContext())
                        {
                            todo.Entry(userCookie).State = EntityState.Modified;
                            todo.SaveChanges();
                        }
                    }
                    else
                    {

                        db.Sessions.Add(new SessionCookie
                        {
                            CookieString = apiCookie.Value,
                            ExpireTime = DateTime.Now.AddDays(20),
                            Email = userD.Email,
                            UserName = userD.Username,

                        });

                        db.SaveChanges();
                    }
                }

                return apiCookie;
            }
        }
        protected void CleanupExpiredSessions()
        {
            using (var db = new SessionContext())
            {
                var expiredSessions = db.Sessions.Where(s => s.ExpireTime <= DateTime.Now).ToList();
                if (expiredSessions.Any())
                {
                    db.Sessions.RemoveRange(expiredSessions);
                    db.SaveChanges();
                }
            }
        }
        protected async Task<ReserPasswordResult> UserResetPasswordDb(string modelEmail, string modelNewPassword)
        {
            using (var db = new UserContext())
            {
                var user = db.UsersT.FirstOrDefault(u => u.Email == modelEmail);
                if (user != null)
                {
                    user.Password = HashPassword.HashPass(modelNewPassword, user.Salt);
            
                    db.Entry(user).State = EntityState.Modified; 
                    await db.SaveChangesAsync();
            
                    return new ReserPasswordResult
                    {
                        Success = true,
                        Message = "Password reset successfully"
                    };
                }
                else
                {
                    return new ReserPasswordResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }
            }
        }


    }
}
