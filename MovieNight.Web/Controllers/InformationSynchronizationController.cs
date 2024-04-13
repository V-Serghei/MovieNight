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
using AutoMapper;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : MasterController
    {
        private readonly ISession _sessionUser;
            
        private readonly IMapper _mapper;
        public InformationSynchronizationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<PEditingM, ProfEditingE>(); 
            });
            _mapper = config.CreateMapper();
        }

        // GET: InformationSynchronization
        [HttpGet]
        public ActionResult PersonalProfile()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            if (_sessionUser.GetUserIdFromSession() == null)
            {

                return View();
            }
            else
            {
                PersonalProfileM personalProfileM = _sessionUser.GetPersonalProfileM(
                    _sessionUser.GetUserIdFromSession());

                PersonalProfileModel model = new PersonalProfileModel
                {
                    AboutMe = personalProfileM.AboutMe,
                    Avatar = personalProfileM.Avatar,
                    BUserModel = new UserModel
                    {
                        Email = personalProfileM.BUserE.Email,
                        Username = personalProfileM.BUserE.Username,
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
                        ViewingHistoryModel tmp = new ViewingHistoryModel
                        {
                            ViewingTime = new TimeModel
                            {
                                Day = personalProfileM.ViewingHistory[0].ViewingTime.Day,
                                Month = personalProfileM.ViewingHistory[0].ViewingTime.Month,
                                Year = personalProfileM.ViewingHistory[0].ViewingTime.Year
                            },
                            Poster = new PosterModel
                            {
                                Id = personalProfileM.ViewingHistory[0].Poster.Id,
                                Name = personalProfileM.ViewingHistory[0].Poster.Name,
                                Path = personalProfileM.ViewingHistory[0].Poster.Path
                            },
                            Star = personalProfileM.ViewingHistory[0].Star,
                            Title = personalProfileM.ViewingHistory[0].Title,
                            Description = personalProfileM.ViewingHistory[0].Description
                        };

                        model.ViewingHistory.Add(tmp);
                        personalProfileM.ViewingHistory.RemoveAt(0);

                    }

                    if (personalProfileM.ViewingHistory.Count == 0
                        && personalProfileM.ListInThePlans.Count == 0) break;
                }
                return View(model);
            }

           
        }

        [HttpGet]
        public ActionResult UserTemplatePage()
        {
            return View();
        }

        [HttpGet]
        public ActionResult MovieTemplatePage()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProfileEditing()
        {



            return View();
        }

        [HttpPost]
        public ActionResult ProfileEdit(PEditingM profEd)
        {


            var profEdBl = _mapper.Map<ProfEditingE>(profEd);

            SuccessOfTheActivity _success = _sessionUser.EdProfInfo(profEdBl);
            if (_success.Successes == true)
            {
                return RedirectToAction("PersonalProfile", "InformationSynchronization");
            }
            else
            {

                return View("ProfileEditing",_success);
            }
        }
    }
}