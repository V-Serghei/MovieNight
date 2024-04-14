﻿using System;
using MovieNight.BusinessLogic.Interface;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Different;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : MasterController
    {
        private readonly ISession _sessionUser;

        private IMovie _movie;
            
        private readonly IMapper _mapper;
        public InformationSynchronizationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();

            var serviceMovieControlBl = new BusinessLogic.BusinessLogic();
            _movie = serviceMovieControlBl.GetMovieService();
            
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<PEditingM, ProfEditingE>();
                cfg.CreateMap<PEditingM, PersonalProfileModel>();
                cfg.CreateMap<PersonalProfileM, PersonalProfileModel>()
                    .ForMember(dist => dist.BUserE,
                        src => src.Ignore());
            });

            _mapper = config.CreateMapper();

        }

        // GET: InformationSynchronization
        [HttpGet]
        public ActionResult PersonalProfile()
        {

            try
            {
                SessionStatus();
                if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
                {
                    return RedirectToAction("Login", "Identification");
                }

                var userHttp = System.Web.HttpContext.Current.GetMySessionObject();

                var userId = _sessionUser.GetUserIdFromSession();
                if (userId != null)
                {
                    var user = _sessionUser.GetPersonalProfileM(userId);
                    if (user != null)
                    {
                        var userM = _mapper.Map<PersonalProfileModel>(user);
                        if (userM != null)
                        {
                            userM.BUserE = new UserModel
                            {
                                Username = userHttp.Username,
                                Email = userHttp.Email
                            };
                             return View(userM);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                var resp = new PersonalProfileModel
                {
                    MsgResp = "Error in processing user information, please try later! Details => "+ex.Message
                    + "Try to re-enter!"
                };
                return View(resp);
            }

            // if (user == null)
            // {
            //     return View();
            // }
            // else
            // {
            //     PersonalProfileM personalProfileM = _sessionUser.GetPersonalProfileM(
            //         _sessionUser.GetUserIdFromSession());
            //
            //     PersonalProfileModel model = new PersonalProfileModel
            //     {
            //         AboutMe = personalProfileM.AboutMe,
            //         Avatar = personalProfileM.Avatar,
            //         BUserModel = new UserModel
            //         {
            //             Email = personalProfileM.BUserE.Email,
            //             Username = personalProfileM.BUserE.Username,
            //         },
            //         Location = personalProfileM.Location,
            //         Quote = personalProfileM.Quote,
            //         ListInThePlans = new List<ListOfFilmsModel>(),
            //         ViewingHistory = new List<ViewingHistoryModel>()
            //     };
            //     while (true)
            //     {
            //         if (personalProfileM.ListInThePlans.Count != 0)
            //         {
            //             ListOfFilmsModel tmp = new ListOfFilmsModel();
            //             tmp.Date = new TimeModel
            //             {
            //                 Day = personalProfileM.ListInThePlans[0].Date.Day,
            //                 Month = personalProfileM.ListInThePlans[0].Date.Month,
            //                 Year = personalProfileM.ListInThePlans[0].Date.Year
            //             };
            //             tmp.Name = personalProfileM.ListInThePlans[0].Name;
            //             tmp.NumberOfViews = personalProfileM.ListInThePlans[0].NumberOfViews;
            //             tmp.Star = personalProfileM.ListInThePlans[0].Star;
            //             tmp.Tags = new List<TagModel>();
            //             foreach (var tag in personalProfileM.ListInThePlans[0].Tags)
            //             {
            //                 tmp.Tags.Add(new TagModel
            //                 {
            //                     Id = tag.Id,
            //                     Name = tag.Name,
            //                 });
            //             }
            //
            //             model.ListInThePlans.Add(tmp);
            //             personalProfileM.ListInThePlans.RemoveAt(0);
            //         }
            //
            //         if (personalProfileM.ViewingHistory.Count != 0)
            //         {
            //             ViewingHistoryModel tmp = new ViewingHistoryModel
            //             {
            //                 ViewingTime = new TimeModel
            //                 {
            //                     Day = personalProfileM.ViewingHistory[0].ViewingTime.Day,
            //                     Month = personalProfileM.ViewingHistory[0].ViewingTime.Month,
            //                     Year = personalProfileM.ViewingHistory[0].ViewingTime.Year
            //                 },
            //                 Poster = new PosterModel
            //                 {
            //                     Id = personalProfileM.ViewingHistory[0].Poster.Id,
            //                     Name = personalProfileM.ViewingHistory[0].Poster.Name,
            //                     Path = personalProfileM.ViewingHistory[0].Poster.Path
            //                 },
            //                 Star = personalProfileM.ViewingHistory[0].Star,
            //                 Title = personalProfileM.ViewingHistory[0].Title,
            //                 Description = personalProfileM.ViewingHistory[0].Description
            //             };
            //
            //             model.ViewingHistory.Add(tmp);
            //             personalProfileM.ViewingHistory.RemoveAt(0);
            //
            //         }
            //
            //         if (personalProfileM.ViewingHistory.Count == 0
            //             && personalProfileM.ListInThePlans.Count == 0) break;
            //     }
            //     return View(model);
            // }
            return View(new PersonalProfileModel{MsgResp = "Oops, you don’t seem to have the right to be here!" +
                                                           "To view this page, you must first log in."});

        }

        [HttpGet]
        public ActionResult UserTemplatePage()
        {
            
            
            return View();
        }

        [HttpGet]
        public ActionResult MovieTemplatePage()
        {
            int? id = 1;
            var movie = _movie.GetMovieInf(id);
            
            
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
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0 && file.ContentType.StartsWith("image/"))
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/uploads/avatars"), fileName);

                    if (!Directory.Exists(Server.MapPath("~/uploads/avatars")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/uploads/avatars"));
                    }

                    file.SaveAs(Path.Combine(Server.MapPath("~/uploads/avatars"), fileName));

                    profEd.Avatar = fileName;
                }
            }



            var profEdBl = _mapper.Map<ProfEditingE>(profEd);

            var success = _sessionUser.EdProfInfo(profEdBl);
            if (success.Successes)
            {
                return RedirectToAction("PersonalProfile", "InformationSynchronization");
            }
            else
            {

                return View("ProfileEditing",success);
            }
        }
    }
}