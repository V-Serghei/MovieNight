using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.PersonalP.Bookmark;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : MasterController
    {
        private readonly ISession _sessionUser;

        private IMovie _movie;
            
        private readonly IMapper _mapper;
        
        private readonly IFriendsService _serviceFriend;
        public InformationSynchronizationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();

            var serviceMovieControlBl = new BusinessLogic.BusinessLogic();
            _movie = serviceMovieControlBl.GetMovieService();

            var serviceFriend = new BusinessLogic.BusinessLogic();
            _serviceFriend = serviceFriend.GetFriendsService();
            
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<PEditingM, ProfEditingE>();
                cfg.CreateMap<PEditingM, PersonalProfileModel>();
                cfg.CreateMap<PersonalProfileM, PersonalProfileModel>()
                    .ForMember(dist => dist.BUserE,
                        src => src.Ignore());
                cfg.CreateMap<MovieTemplateInfE, MovieTemplateInfModel>()
                    .ForMember(cnf => cnf.CastMembers,
                        src => src.Ignore())
                    .ForMember(cnf => cnf.InterestingFacts,
                        src => src.Ignore())
                    .ForMember(cnf => cnf.MovieCards,
                        src => src.Ignore());
                cfg.CreateMap<InterestingFact, InterestingFactE>();
                cfg.CreateMap<InterestingFactE, InterestingFact>();
                cfg.CreateMap<MovieCardE, MovieCard>();
                cfg.CreateMap<MovieCard, MovieCardE>();
                cfg.CreateMap<CastMemberE, CastMember>();
                cfg.CreateMap<CastMember, CastMemberE>();
                cfg.CreateMap<ListOfFilmsE, ListOfFilmsModel>();
                cfg.CreateMap<ListOfFilmsModel, ListOfFilmsE>();
                cfg.CreateMap<ViewingHistoryM,ViewingHistoryModel>();
                cfg.CreateMap<ViewingHistoryModel,ViewingHistoryM>();


                cfg.CreateMap<FriendsPageD, FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore());
            });

            _mapper = config.CreateMapper();

        }

        // GET: InformationSynchronization
        [HttpGet]
        [UserMod]
        public ActionResult PersonalProfile()
        {

            try
            {
                SessionStatus();
                if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
                {
                    return View();
                }
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
                            var listInPl = _movie.GetListPlain(userId);
                            userM.ListInThePlans = _mapper.Map<List<ListOfFilmsModel>>(listInPl);
                            var lisrViewing = _movie.GetViewingList(userId);
                            userM.ViewingHistory = _mapper.Map<List<ViewingHistoryModel>>(lisrViewing);
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
            int? id = 1;
            var friendsDate = _serviceFriend.getFriendDate(id);
            var friendmodel = _mapper.Map<FriendPageModel>(friendsDate);
            if (friendmodel != null)
            {
                friendmodel.BUserE = new UserModel
                {
                    Username = friendsDate.BUserE.Username,
                    Email = friendsDate.BUserE.Email
                };
                return View(friendmodel);
            }
            return View();
        }

        
        
        [HttpGet]
        public ActionResult MovieTemplatePage(int? id)
        {
            int idU = 0;
            if (id != null)
            {
                idU = (int)id;
            }

            try
            {
                var movie = _movie.GetMovieInf(id);
                if (movie != null)
                {
                    var movieModel = _mapper.Map<MovieTemplateInfModel>(movie);
                    movieModel.MovieCards = _mapper.Map<List<MovieCard>>(movie.MovieCards);
                    movieModel.CastMembers = _mapper.Map<List<CastMember>>(movie.CastMembers);
                    movieModel.InterestingFacts = _mapper.Map<List<InterestingFact>>(movie.InterestingFacts);
                    movieModel.Genre = new List<string>();
                    movieModel.Bookmark = _movie.GetInfBookmark((System.Web.HttpContext.
                        Current.GetMySessionObject().Id,idU));
                    movieModel.UserRating =
                        _movie.GetUserRating((System.Web.HttpContext.Current.GetMySessionObject().Id, idU));
                    foreach (var GEN in movie.Genre)
                    {
                        movieModel.Genre.Add(GEN);
                    }

                    movieModel.Id = idU;
                    return View(movieModel);
                }

                // return View("Error404", "Error");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //return View("Error500", "Error");
            }

            return View();
        }

        [HttpGet]
        public ActionResult ProfileEditing()
        {
            var used = _sessionUser.GetPersonalProfileM(HttpContextInfrastructure.GetGlobalParam());
            
            var model = _mapper.Map<PersonalProfileModel>(used);
            var userCurr = System.Web.HttpContext.Current.GetMySessionObject();
            if (model != null && model.BUserE == null)
            {
                model.BUserE = new UserModel
                {
                    Username = userCurr.Username,
                    Email = userCurr.Email
                };
            }
            if(model!=null) return View(model);
            return View(new PersonalProfileModel
            {
                MsgResp = "Things go wrong!!!",
                BUserE = new UserModel
                {
                    Username = userCurr.Username,
                    Email = userCurr.Email
                }
            });
        }

        [HttpPost]
        [UserMod]
        public ActionResult ProfileEdit(PEditingM profEd)
        {
            if (profEd.AvatarFile != null)
            {
                
                var filePath = Path.Combine(Server.MapPath("~/uploads/avatars"), profEd.AvatarFile.FileName);
                profEd.AvatarFile.SaveAs(filePath);
                profEd.Avatar = "~/uploads/avatars/" + profEd.AvatarFile.FileName;

            }

            var userCurr = System.Web.HttpContext.Current.GetMySessionObject();
            var profEdBl = _mapper.Map<ProfEditingE>(profEd);

            var success = _sessionUser.EdProfInfo(profEdBl);
            if (success.Successes)
            {
                return RedirectToAction("PersonalProfile", "InformationSynchronization");
            }

            PersonalProfileModel resp = _mapper.Map<PersonalProfileModel>(success.InfOfUser);
            if (resp != null)
            {
                resp.MsgResp = success.Msg;
                return View("ProfileEditing",resp);
            }
            return View("ProfileEditing", new PersonalProfileModel
            {
                MsgResp = success.Msg,
                BUserE = new UserModel
                {
                    Username = userCurr.Username,
                    Email = userCurr.Email
                }
            });
        }

        
        [HttpPost]
        public async Task<JsonResult> BookmarkMovie(int movieId)
        {   
            var bookMe = await _movie.SetNewBookmark((System.Web.HttpContext.
                Current.GetMySessionObject().Id,movieId));
                var bookM = new BookmarkModel
            {
                IdUser = bookMe.IdUser,
                IdMovie = bookMe.IdMovie,
                Msg = bookMe.Msg,
                Success = bookMe.Success

            };
            
            
            
            return Json(new { success = true, Msg = "StatusMsg", newButtonColor = "red", bookM}); 
        }
        
        [HttpPost]
        public async Task<JsonResult> DeleteBookmarkMovie(int movieId)
        {   
            var deleteBookmark = await _movie.DeleteBookmark((System.Web.HttpContext.
                Current.GetMySessionObject().Id,movieId));

            if (deleteBookmark)
            {
                return Json(new { success = true, Msg = "Deleted"}); 

            }

            return Json(new { success = false, Msg = "Error"});


        }
        
        public async Task<JsonResult> AddToViewed(int movieid)
        {
            return Json(new { success = true, statusMsg = "StatusMsg", newButtonTitle = "Новое сообщение", newButtonColor = "red" }); 
        }

        public async Task<JsonResult> AddToGrade()
        {
            return Json(new { success = true, statusMsg = "StatusMsg", newButtonTitle = "Новое сообщение", newButtonColor = "red" }); 
        }

        [HttpPost]
        public async Task<JsonResult> RateMovie(int rating, int movieId)
        {

            var resp =  await _movie.SetReteMovieAndView((System.Web.HttpContext.Current.GetMySessionObject().Id, movieId,
                rating));
            
            
            
            return (Json(new { star = rating }));
        }
    }
}