using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.DifferentE;

using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Xml.Linq;
using System.Web;

namespace MovieNight.BusinessLogic.Core
{
    public class UserApi
    {
        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //  - > password-related methods     
        //  |  |  |  |  |  |  |  |  |  |  |  |
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/
        public string HashPassword(string password, string salt)
        {
            var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);

            var hashedPassword = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(24));
            return hashedPassword;
        }

        public bool VerifyPassword(string providedPassword, string storedHash, string salt)
        {
            var newHash = HashPassword(providedPassword, salt);
            return newHash == storedHash;
        }
        
        private string GetRandSalt()
        {
            var rng = new RNGCryptoServiceProvider();
            var salt = new byte[16];
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);

        }

        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //           - > login  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/

        public bool IsValid(LogInData rData)
        {
            if (string.IsNullOrEmpty(rData.Password) || string.IsNullOrEmpty(rData.Email))
                return false;

            // Regex for email validation
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(rData.Email))
                return false;

            // Minimum password length
            if (rData.Password.Length < 10)
                return false;

            return true;
        }

        public async Task<UserVerification> GetUserVerification(LogInData logInData)
        {
            //check if user exist
            var userL = new UserVerification();

            if (!IsValid(logInData))
            {
                userL.StatusMsg = "Invalid data";
                userL.IsVerified = false;
                return userL;
            }

            using (var db = new UserContext())
            {
                var userExists = await db.UsersT.FirstOrDefaultAsync(u => u.Email == logInData.Email);

                if (userExists == null)
                {
                        userL.StatusMsg = "You have entered unrecorded mail";
                        userL.IsVerified = false;
                    return userL;
                }else if (!VerifyPassword(logInData.Password, userExists.Password, userExists.Salt))
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
                    HttpContext.Current.Session["UserId"] = userExists.Id;
                    HttpContext.Current.Session["UserName"] = userExists.UserName;
                    return userL;
                }
            }

        }


        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //       - > registration  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/
        public bool IsValid(RegData rData)
        {
            if (string.IsNullOrEmpty(rData.UserName) || string.IsNullOrEmpty(rData.Password) || string.IsNullOrEmpty(rData.Email))
                return false;

            // Regex for email validation
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(rData.Email))
                return false;

            // Minimum password length
            if (rData.Password.Length < 10)
                return false;

            return true;
        }

        public async Task<UserRegister> AddNewUserSuccess(RegData rData)
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
                    if (userExists.UserName == rData.UserName)
                        userRegister.StatusMsg = "Username already taken";
                    else
                        userRegister.StatusMsg = "Email already in use";

                    return userRegister;
                }
            }

            userRegister.SuccessUniq = true;
            userRegister.StatusMsg = "Success";
            return userRegister;
        }

        public bool UserAdding(RegData rData)
        {
            //add user to database

            var user = new UserDbTable()
            {
                UserName = rData.UserName,
                Email = rData.Email,
                LastLoginDate = rData.RegDateTime,
                LastIp = rData.Ip,
                Role = LevelOfAccess.User,
                Checkbox = rData.Checkbox,
                Salt = GetRandSalt()

            };
            user.Password = HashPassword(rData.Password, user.Salt);
            //using (var db = new UserContext())
            //{
            //    var us = db.UsersT.FirstOrDefault(u => u.UserName == rData.UserName);
            //}

            using (var db = new UserContext())
            {
                try
                {
                    db.UsersT.Add(user);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }



        // /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
        //
        //        - > user read  < -   
        //  
        // \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/ \/

        public UserE GetUserDataFromDatabase(int? userId)
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

        public PersonalProfileM GetPersonalProfileDatabase(int? userId)
        {
            //search the database


            PersonalProfileM personalProfileM = new PersonalProfileM
            {
                Avatar = "~/images/users/photo_2023-03-30_21-08-09.jpg",
                BUserE = new UserE
                {
                    Email = GetUserDataFromDatabase(userId).Email, 
                    Username= GetUserDataFromDatabase(userId).Username,
                },
                Quote = "Movie fan",
                AboutMe = "I’m Nelly and I love watching movies. Especially anime. For me, nothing is better than anime. Yes, and I’m also a cool IT girl and designer. And I’m also a master. I can do everything. " +
                          "Cook, sew, draw, sculpt. I can learn everything. The main thing is to want.",
                Number = new PhoneNumE
                {
                    Number = 69892856,
                    CountryС = 373
                },
                Location = "Moldova",
                ViewingHistory = new List<ViewingHistoryM>(),
                ListInThePlans = new List<ListOfFilms>()

            };
            //Get out of the movie database
            for (int i = 0; i < 5; i++)
            {
                personalProfileM.ListInThePlans.Add(new ListOfFilms
                {
                    Date = new TimeD
                    {
                        Day = 10+i,
                        Month = 10,
                        Year = 2024
                    },
                    Name = "The Shawshank Redemption",
                    NumberOfViews = 1102031331,
                    Star = 5+i,
                    Tags = new List<Tag>
                    {
                        new Tag
                        {
                            Id = 15+i,
                            Name = "drama"
                        },
                        new Tag
                        {
                        Id = 12+i,
                        Name = "family"
                    }
                    }
                    
                });
                

            }
            for (int i = 0; i < 5; i++)
            {
                personalProfileM.ViewingHistory.Add(new ViewingHistoryM
                {
                    Description = "",
                    Id = i,
                    Title = "Solo Leveling",
                    Poster = new Poster
                    {
                        Id = i,
                        Name = "Solo Leveling",
                        Path = "~/images/index2.jpg",
                    },
                    Star = i+4,
                    ViewingTime = DateTime.Now
                   
                });
            }

            return personalProfileM;
        }
    }
}
