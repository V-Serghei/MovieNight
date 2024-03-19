using MovieNight.BusinessLogic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Different;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : Controller
    {
        internal ISession SessionUser;
        public InformationSynchronizationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            SessionUser = sesControlBl.Session();
        }

        // GET: InformationSynchronization
        public ActionResult PersonalProfile()
        {
            PersonalProfileM personalProfileM = SessionUser.GetPersonalProfileM(
                SessionUser.GetUserIdFromSession());

            PersonalProfileModel model = new PersonalProfileModel
            {
                AboutMe = personalProfileM.AboutMe,
                Avatar = personalProfileM.Avatar,
                BUserModel = new UserModel
                {
                    Email = personalProfileM.BUserE.Email,
                    Name = personalProfileM.BUserE.Name,
                },
                Location = personalProfileM.Location,
                Quote = personalProfileM.Quote,
                Number = new PhoneNumModel
                {
                    CountryС = personalProfileM.Number.CountryС,
                    Number = personalProfileM.Number.Number
                },
                ListInThePlans = new List<ListOfFilmsModel>(),
                ViewingHistory = new List<ViewingHistoryModel>()
            };
            while (true)
            {
                if (personalProfileM.ListInThePlans.Count != 0)
                {
                    ListOfFilmsModel tmp = new ListOfFilmsModel();
                    tmp.Date = new TimeModel
                    {
                        Day = personalProfileM.ListInThePlans[0].Date.Day,
                        Month = personalProfileM.ListInThePlans[0].Date.Month,
                        Year = personalProfileM.ListInThePlans[0].Date.Year
                    };
                    tmp.Name = personalProfileM.ListInThePlans[0].Name;
                    tmp.NumberOfViews = personalProfileM.ListInThePlans[0].NumberOfViews;
                    tmp.Star = personalProfileM.ListInThePlans[0].Star;
                    tmp.Tags = new List<TagModel>();
                    foreach (var tag in personalProfileM.ListInThePlans[0].Tags)
                    {
                        tmp.Tags.Add(new TagModel
                        {
                            Id = tag.Id,
                            Name = tag.Name,
                        });
                    }
                    model.ListInThePlans.Add(tmp);
                    personalProfileM.ListInThePlans.RemoveAt(0);
                }

                if (personalProfileM.ViewingHistory.Count != 0)
                {
                    ViewingHistoryModel tmp = new ViewingHistoryModel();
                    tmp.ViewingTime = new TimeModel
                    {
                        Day = personalProfileM.ViewingHistory[0].ViewingTime.Day,
                        Month = personalProfileM.ViewingHistory[0].ViewingTime.Month,
                        Year = personalProfileM.ViewingHistory[0].ViewingTime.Year
                    };
                    tmp.Poster = new PosterModel
                    {
                        Id = personalProfileM.ViewingHistory[0].Poster.Id,
                        Name = personalProfileM.ViewingHistory[0].Poster.Name,
                        Path = personalProfileM.ViewingHistory[0].Poster.Path
                    };
                    tmp.Star = personalProfileM.ViewingHistory[0].Star;
                    tmp.Title = personalProfileM.ViewingHistory[0].Title;
                    tmp.Description = personalProfileM.ViewingHistory[0].Description;
                    
                    model.ViewingHistory.Add(tmp);
                    personalProfileM.ViewingHistory.RemoveAt(0);

                }
                if(personalProfileM.ViewingHistory.Count == 0 
                   && personalProfileM.ListInThePlans.Count == 0)break;
            }


            return View(model);
        }

        public ActionResult UserTemplatePage()
        {
            return View();
        }

        public ActionResult MovieTemplatePage()
        {
            return View();
        }
    }
}