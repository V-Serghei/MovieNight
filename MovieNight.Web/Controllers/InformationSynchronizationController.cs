using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Achievement;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.PersonalP.Bookmark;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.Review;
using MovieNight.Web.Models.Review;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : MasterController
    {
        private readonly ISession _sessionUser;

        private readonly IMovie _movie;
            
        private readonly IMapper _mapper;
        
        private readonly IFriendsService _serviceFriend;
        
        private readonly IAchievements _achievements;
        public InformationSynchronizationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();

            var serviceMovieControlBl = new BusinessLogic.BusinessLogic();
            _movie = serviceMovieControlBl.GetMovieService();

            var serviceFriend = new BusinessLogic.BusinessLogic();
            _serviceFriend = serviceFriend.GetFriendsService();
            
            var serviceAchievements = new BusinessLogic.BusinessLogic();
            _achievements = serviceAchievements.GetAchievementsService();
            
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
                cfg.CreateMap<AchievementModel, AchievementE>();
                cfg.CreateMap<AchievementE, AchievementModel>();


                cfg.CreateMap<FriendsPageD, FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore());
            });

            _mapper = config.CreateMapper();

        }

       
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
                            var statistic = _movie.GetDataStatisticPage(userId);
                            if (statistic != null)
                            {
                                userM.AnimeCount = statistic.AnimeCount;
                                userM.AnimeTotal = statistic.AnimeTotal;
                                userM.CartonsCount = statistic.CartonsCount;
                                userM.CartonTotal = statistic.CartonTotal;
                                userM.FilmCount = statistic.FilmCount;
                                userM.FilTotal = statistic.FilTotal;
                                userM.SerialsCount = statistic.SerialsCount;
                                userM.SerialTotal = statistic.SerialTotal;
                            }
                            var achievements = _achievements.GetAchievements(userId);
                            if (achievements != null)
                            {
                                var achiev = _mapper.Map<List<AchievementModel>>(achievements);
                                if (achiev != null)
                                {
                                    userM.Achievements = achiev;
                                }
                            }
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
        [UserMod]
        public ActionResult UserTemplatePage(int? id)
        {
            var friendsDate = _serviceFriend.getFriendDate(id);
            var friendmodel = _mapper.Map<FriendPageModel>(friendsDate);
            if (friendmodel != null)
            {
                {
                    var listInPl = _movie.GetListPlain(id);
                    friendmodel.ListInThePlans = _mapper.Map<List<ListOfFilmsModel>>(listInPl);
                    var lisrViewing = _movie.GetViewingList(id);
                    friendmodel.ViewingHistory = _mapper.Map<List<ViewingHistoryModel>>(lisrViewing);
                    var defInf = _sessionUser.GetUserData(id);
                    friendmodel.BUserE = new UserModel
                    {
                        Username = defInf.Username,
                        Email = defInf.Email
                    };
                    var statistic = _movie.GetDataStatisticPage(id);
                    if (statistic != null)
                    {
                        friendmodel.AnimeCount = statistic.AnimeCount;
                        friendmodel.AnimeTotal = statistic.AnimeTotal;
                        friendmodel.CartonsCount = statistic.CartonsCount;
                        friendmodel.CartonTotal = statistic.CartonTotal;
                        friendmodel.FilmCount = statistic.FilmCount;
                        friendmodel.FilTotal = statistic.FilTotal;
                        friendmodel.SerialsCount = statistic.SerialsCount;
                        friendmodel.SerialTotal = statistic.SerialTotal;
                    }

                    var achievements = _achievements.GetAchievements(id);
                    if (achievements != null)
                    {
                        var achiev = _mapper.Map<List<AchievementModel>>(achievements);
                        if (achiev != null)
                        {
                            friendmodel.Achievements = achiev;
                        }
                    }
                }
            }

            return View(friendmodel);
        }

        
        
        [HttpGet]
        public ActionResult MovieTemplatePage(int? id)
        {
            SessionStatus();
            
          
            
            
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
                    if(System.Web.HttpContext.Current.GetMySessionObject()!=null){movieModel.Bookmark = _movie.GetInfBookmark((System.Web.HttpContext.
                        Current.GetMySessionObject()?.Id??null,idU));
                    
                        movieModel.UserRating =
                            _movie.GetUserRating((System.Web.HttpContext.Current.GetMySessionObject().Id, idU));
                    }
                    foreach (var GEN in movie.Genre)
                    {
                        movieModel.Genre.Add(GEN);
                    }

                    movieModel.Id = idU;
                    return View(movieModel);
                }

                // return View("Error404", "Error");

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($@"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                return null;
            }

            return View();
        }

        [HttpGet]
        [UserMod]
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
        public async Task< ActionResult> ProfileEdit(PEditingM profEd)
        {
            SessionStatus();
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
                var achievement = await _achievements.AchievementСheck((userCurr.Id, AchievementType.CompleteProfile));
                if (achievement != null && achievement.Unlocked)
                {
                    var achiev = _mapper.Map<AchievementModel>(achievement);
                    System.Web.HttpContext.Current.SetListAchievement(achiev);
                }
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
                Success = bookMe.Success,
                BookmarkTimeOf = bookMe.BookmarkTimeOf,
                BookMark = bookMe.BookMark

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
        public async Task<JsonResult> DeleteBookmarkTimeOf(int movieId)
        {   
            var deleteBookmark = await _movie.DeleteBookmarkTimeOf((System.Web.HttpContext.
                Current.GetMySessionObject().Id,movieId));
            
            if (System.Web.HttpContext.Current.VerifyExistBookmark(new BookmarkModel{IdMovie = movieId}))
            {
                System.Web.HttpContext.Current.RemoveBookmark(new BookmarkModel{IdMovie = movieId});
            }
            if (deleteBookmark)
            {
                return Json(new { success = true, Msg = "Deleted"}); 

            }

            return Json(new { success = false, Msg = "Error"});


        }

        public async Task<JsonResult> AddToBookmarkTimeOf(int movieId)
        {
            var resp = await _movie.SetNewBookmarkTimeOf((System.Web.HttpContext.Current.GetMySessionObject().Id, movieId));
            // if (System.Web.HttpContext.Current.GetBookmarkTimeOf() == null)
            // {
                var bookmarkTest = new BookmarkModel
                {
                    IdUser = resp.Bookmark.IdUser,
                    IdMovie = resp.Bookmark.IdMovie,
                    BookmarkTimeOf = resp.Bookmark.BookmarkTimeOf,
                    TimeAdd = resp.Bookmark.TimeAdd
                };
                
                if (!System.Web.HttpContext.Current.VerifyExistBookmark(bookmarkTest))
                {
                    var getLostB = System.Web.HttpContext.Current.GetBookmarkTimeOf();
                    getLostB.MovieInTimeOfBookmark.Add(_mapper.Map<MovieTemplateInfModel>(resp.MovieInTimeOfBookmark));
                    getLostB.Bookmark.Add(bookmarkTest);
                    System.Web.HttpContext.Current.SetBookmarkTimeOf(getLostB);
                }
                
                
            // }
            // else
            // {
            //     var bookmarkTimeOf = System.Web.HttpContext.Current.GetBookmarkTimeOf();
            //     bookmarkTimeOf.MovieInTimeOfBookmark.Add(_mapper.Map<MovieTemplateInfModel>(resp.MovieInTimeOfBookmark));
            //     bookmarkTimeOf.Bookmark.Add(new BookmarkModel
            //     {
            //         IdUser = resp.Bookmark.IdUser,
            //         IdMovie = resp.Bookmark.IdMovie,
            //         BookmarkTimeOf = resp.Bookmark.BookmarkTimeOf,
            //         TimeAdd = resp.Bookmark.TimeAdd
            //
            //     });
            //     System.Web.HttpContext.Current.SetBookmarkTimeOf(bookmarkTimeOf);
            // }
            
            return Json(new { success = resp.IsSuccese, Msg = resp.RespMsg, newButtonTitle = "Delete", newButtonColor = "red" }); 

        }
        
        public async Task<ActionResult> AddToViewed(int? movieId)
        {

            if (movieId != null)
            {
                var respAddViewed = await 
                    _movie.SetViewList((movieId, System.Web.HttpContext.Current.GetMySessionObject()?.Id));
                if (respAddViewed.IsSuccese)
                {
                    var achievement = await _achievements.AchievementСheck((System.Web.HttpContext.Current.GetMySessionObject()?.Id, AchievementType.FirstMovie));
                    if (achievement != null && achievement.Unlocked)
                    {
                        var achiev = _mapper.Map<AchievementModel>(achievement);
                        System.Web.HttpContext.Current.SetListAchievement(achiev);
                    }
                    return  RedirectToAction("MovieTemplatePage",new {id = movieId}); 
                }
                else
                {
                    return  RedirectToAction("MovieTemplatePage",new {id = movieId}); 
                }
            }
            

            return RedirectToAction("Error404Page", "Error");

        }

        // public async Task<JsonResult> AddToGrade()
        // {
        //     return Json(new { success = true, statusMsg = "StatusMsg", newButtonTitle = "Новое сообщение", newButtonColor = "red" }); 
        // }

        [HttpPost]
        public async Task<JsonResult> RateMovie(int rating, int movieId)
        {

            var resp =  await _movie.SetReteMovieAndView((System.Web.HttpContext.Current.GetMySessionObject().Id, movieId,
                rating));

            if (resp)
            {
                var achievement = await _achievements.AchievementСheck((System.Web.HttpContext.Current.GetMySessionObject()?.Id, AchievementType.FirstMovie));
                if (achievement != null && achievement.Unlocked)
                {
                    var achiev = _mapper.Map<AchievementModel>(achievement);
                    System.Web.HttpContext.Current.SetListAchievement(achiev);
                } 
            }
            
            return (Json(new { star = rating }));
        }
        
        [GuestMod]
        public ActionResult MoviePlayer(int? movieId)
        {
            int idU = 0;
            if (movieId != null)
            {
                idU = (int)movieId;
            }

            try
            {
                var movie = _movie.GetMovieInf(movieId);
                if (movie != null)
                {
                    var movieModel = _mapper.Map<MovieTemplateInfModel>(movie);
                    movieModel.MovieCards = _mapper.Map<List<MovieCard>>(movie.MovieCards);
                    movieModel.CastMembers = _mapper.Map<List<CastMember>>(movie.CastMembers);
                    movieModel.InterestingFacts = _mapper.Map<List<InterestingFact>>(movie.InterestingFacts);
                    movieModel.Genre = new List<string>();
                    if(System.Web.HttpContext.Current.GetMySessionObject()!=null){
                        movieModel.Bookmark =
                            _movie.GetInfBookmark((System.Web.HttpContext.Current.GetMySessionObject().Id, idU));
                        movieModel.UserRating =
                            _movie.GetUserRating((System.Web.HttpContext.Current.GetMySessionObject().Id, idU));
                    }
                    foreach (var GEN in movie.Genre)
                    {
                        movieModel.Genre.Add(GEN);
                    }

                    movieModel.Id = idU;
                    return View(movieModel);
                }
                return RedirectToAction("Error404Page", "Error");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return RedirectToAction("Error404Page", "Error");

            }

        }
        [HttpPost]
        public JsonResult ClearBookmarks()
        {
            try
            {
                _movie.ClearBookmarks();
                System.Web.HttpContext.Current.GetBookmarkTimeOf().Bookmark.Clear();
                System.Web.HttpContext.Current.GetBookmarkTimeOf().MovieInTimeOfBookmark.Clear();
        
                return Json(new { success = true, message = "Bookmarks cleared successfully" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "An error occurred: " + e.Message });
            }
        }

        
        [HttpGet]
       
        public ActionResult ReviewPage(int? filmId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ReviewE, ReviewModel>();
            });
            var mapper = config.CreateMapper();
            List<ReviewE> reviews = _movie.getListOfReviews(filmId);
            var review = mapper.Map<List<ReviewModel>>(reviews);
            var model = new ReviewPageModel()
            {
                FilmId = filmId,
                RGreat = review.Where(g=>g.ReviewType == TypeOfReview.Great).ToList(),
                RFine = review.Where(g=>g.ReviewType == TypeOfReview.Fine).ToList(),
                RWaste = review.Where(g=>g.ReviewType == TypeOfReview.Waste).ToList(),
                FilmTitle = _movie.GetMovieInf(filmId).Title
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult ReviewPageWrite(ReviewModel movieReview)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ReviewModel, ReviewE>();
            });
            var mapper = config.CreateMapper();
            var review = mapper.Map<ReviewE>(movieReview);
            review.Date = DateTime.Now;
            review.UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id;
            review.User = System.Web.HttpContext.Current.GetMySessionObject()?.Username;
            bool addReview = _movie.setNewReview(review);
            if (addReview)
            {
                return RedirectToAction("ReviewPage", new{filmId = review.FilmId});
            }
            return RedirectToAction("Error404Page","Error");
        }
        [ModeratorMod]
        public ActionResult DeleteReviewData(int? movieId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ReviewModel, ReviewE>();
            });
            var mapper = config.CreateMapper();
            int? deleteReview = _movie.DeleteReview(movieId);
            if (deleteReview!=null)
            {
                return RedirectToAction("ReviewPage", new{filmId = deleteReview});
            }
            return RedirectToAction("Error404Page","Error");
        }

        [ModeratorMod]
        public ActionResult MovieTemplateEditing()
        {
            return View();
        }
    }
}