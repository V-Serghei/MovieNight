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

namespace MovieNight.BusinessLogic.Core
{
    public class UserApi
    {
        public UserVerification GetUserVerification(LogInData logInData)
        {

            //check if user exist

            using (var db = new UserContext())
            {
                var user = db.UsersT.FirstOrDefault(u=> u.UserName == logInData.Username);
            }
            object ss = null;



            var exUserAip = new UserVerification();
            //database search logic

            if(logInData.Username == "vistovschii@gmail.com" && logInData.Password == "1111") 
            {
                exUserAip.IsVerified = true;
                exUserAip.LogInData = logInData;
            }else
            {
                exUserAip.IsVerified = false;
            }
            return exUserAip;
        }

        public UserRegister AddNewUserSuccess(RegData rData)
        {
            var exUserAip = new UserRegister();

            //check the database for such a user

            if (rData.FullName != "" && rData.Password != "" && rData.Email != "")
            {
                exUserAip.SuccessUniq = true;
            }
            else exUserAip.SuccessUniq = false;
            return exUserAip;
        }

        public bool UserAdding(RegData rData)
        {
            //add user to database
            var user = new UserDbTable()
            {
                UserName = rData.FullName,
                Password = rData.Password,
                Email = rData.Email,
                LastLoginDate = rData.RegDateTime,
                LastIp = rData.Ip,
                Role = LevelOfAccess.User,
                Checkbox = rData.Checkbox

            };
            using (var db = new UserContext())
            {
                var us = db.UsersT.FirstOrDefault(u => u.UserName == rData.FullName);
            }

            using (var db = new UserContext())
            {
                db.UsersT.Add(user);
                db.SaveChanges();
            }

            if (rData.FullName != null && rData.Password != null && rData.Email != null)
            {
                return true;
            }
            return false;
        }
        public UserE GetUserDataFromDatabase(int? userId)
        {
            // Fetch user data from database 
            UserE userE = null;
            if (userId != null)
            {
                userE = new UserE
                {
                    Name = "Nelly",
                    Email = "Nelly@gmail.com",
                    //Id = userId,
                    Password = "1111"

                };
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
                    Name = GetUserDataFromDatabase(userId).Name,
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
                    ViewingTime = new TimeD
                    {
                        Day = 02+i,
                        Month = 03+i,
                        Year = 2024
                    },
                });
            }

            return personalProfileM;
        }
    }
}
