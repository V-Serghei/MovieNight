using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.ResultsOfTheOperation;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Attributes;
using MovieNight.Web.Models.Achievement;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;

namespace MovieNight.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : MasterController
    {
        #region Basic Settings
        
        private readonly ISession _sessionUser;

        private readonly IMovie _movie;
            
        private readonly IMapper _mapper;
        
        private readonly IFriendsService _serviceFriend;
        
        private readonly IAchievements _achievements;
        public AdminController()
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
                
                cfg.CreateMap<MovieTemplateInfModel, MovieTemplateInfE>();
                cfg.CreateMap<InterestingFact, InterestingFactE>();
                cfg.CreateMap<InterestingFactE, InterestingFact>();
                cfg.CreateMap<MovieCardE, MovieCard>();
                cfg.CreateMap<MovieCard, MovieCardE>();
                cfg.CreateMap<CastMemberE, CastMember>();
                cfg.CreateMap<CastMember, CastMemberE>();
                cfg.CreateMap<MovieTemplateInfE, MovieTemplateInfModel>()
                    .ForMember(cnf => cnf.CastMembers,
                        src => src.MapFrom(sr=>sr.CastMembers))
                    .ForMember(cnf => cnf.InterestingFacts,
                        src => src.MapFrom(sr=>sr.InterestingFacts))
                    .ForMember(cnf => cnf.MovieCards,
                        src => src.MapFrom(sr=>sr.MovieCards))
                    .ForMember(cnf=>cnf.MovieFriends,
                        src=>src.Ignore());
                cfg.CreateMap<ListOfFilmsE, ListOfFilmsModel>();
                cfg.CreateMap<ListOfFilmsModel, ListOfFilmsE>();
                cfg.CreateMap<ViewingHistoryM,ViewingHistoryModel>();
                cfg.CreateMap<ViewingHistoryModel,ViewingHistoryM>();
                cfg.CreateMap<AchievementModel, AchievementE>();
                cfg.CreateMap<AchievementE, AchievementModel>();
                cfg.CreateMap<ScoresFriendsGaveTheMovieE, ScoresFriendsGaveTheMovieModel>();
                cfg.CreateMap<ScoresFriendsGaveTheMovieModel, ScoresFriendsGaveTheMovieE>();

                cfg.CreateMap<FriendsPageD, FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore());
            });

            _mapper = config.CreateMapper();

        }

        #endregion

        #region File Uploading
        
        [HttpPost]
        [AdminMod]
        public ActionResult UploadingTheFileOfAddingMoviesDb(HttpPostedFileBase file)
        {
            if (file != null)
            {
                
                var filePath = Path.Combine(Server.MapPath("~/uploads"), file.FileName);
                file.SaveAs(filePath);

            }

            return View("Index");
        }

        [HttpGet]
        [AdminMod]
        public ActionResult Index()
        {

            return View();
        }
        
        #endregion
        
        #region Movie Template editing
        [ModeratorMod]
        [HttpGet]
        public ActionResult MovieTemplateEditing()
        {
            return View();
        }

        [ModeratorMod]
        [HttpPost]
        public ActionResult MovieTemplateAdding(
            MovieTemplateInfModel model, 
            HttpPostedFileBase AvatarFile, 
            IEnumerable<HttpPostedFileBase> CastImages, 
            IEnumerable<HttpPostedFileBase> CardImages)
        {
            SessionStatus();
            
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error404Page","Error");
            }

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string posterPath = Path.Combine(Server.MapPath("~/images/Movie"), Path.GetFileName(AvatarFile.FileName));
                AvatarFile.SaveAs(posterPath);
                model.PosterImage = "/images/Movie/" + Path.GetFileName(AvatarFile.FileName);
            }

            int castIndex = 0;
            foreach (var file in CastImages)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string castImagePath = Path.Combine(Server.MapPath("~/images/Cast"), Path.GetFileName(file.FileName));
                    file.SaveAs(castImagePath);
                    model.CastMembers[castIndex].ImageUrl = "/images/Cast/" + Path.GetFileName(file.FileName);
                }
                castIndex++;
            }

            int cardIndex = 0;
            foreach (var file in CardImages)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string cardImagePath = Path.Combine(Server.MapPath("~/images/Cards"), Path.GetFileName(file.FileName));
                    file.SaveAs(cardImagePath);
                    model.MovieCards[cardIndex].ImageUrl = "/images/Cards/" + Path.GetFileName(file.FileName);
                }
                cardIndex++;
            }
            var movieData = _mapper.Map<MovieTemplateInfE>(model);
            var result = _movie.AddMovieTemplate(movieData);
            if (result.Result)
            {
                var idM =  _movie.GetMovieId(movieData);
                return RedirectToAction("MovieTemplatePage",
                    "InformationSynchronization",
                    new { id = idM });

            }
            else
            {
                return RedirectToAction("Error404Page","Error");
            }
        }
        
        [ModeratorMod]
        [HttpGet]
        public ActionResult MovieTemplateModify(int? id)
        {
            SessionStatus();
            var movieTemplate = _movie.GetMovieInf(id);
            if (movieTemplate == null) return RedirectToAction("Error404Page", "Error");
            var movieModel = _mapper.Map<MovieTemplateInfModel>(movieTemplate);
            return View(movieModel);
        }

        [ModeratorMod]
        [HttpPost]
        public ActionResult MovieEditing(
            MovieTemplateInfModel model, 
            HttpPostedFileBase AvatarFile, 
            IEnumerable<HttpPostedFileBase> CastImages, 
            IEnumerable<HttpPostedFileBase> CardImages) 
        {
            SessionStatus();

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error404Page", "Error");
            }

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string posterPath = Path.Combine(Server.MapPath("~/images/Movie"),
                    Path.GetFileName(AvatarFile.FileName));
                AvatarFile.SaveAs(posterPath);
                model.PosterImage = "/images/Movie/" + Path.GetFileName(AvatarFile.FileName);
            }

            int castIndex = 0;
            foreach (var file in CastImages)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string castImagePath = Path.Combine(Server.MapPath("~/images/Cast"),
                        Path.GetFileName(file.FileName));
                    file.SaveAs(castImagePath);
                    model.CastMembers[castIndex].ImageUrl = "/images/Cast/" + Path.GetFileName(file.FileName);
                }
                castIndex++;
            }

            int cardIndex = 0;
            foreach (var file in CardImages)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string cardImagePath = Path.Combine(Server.MapPath("~/images/Cards"),
                        Path.GetFileName(file.FileName));
                    file.SaveAs(cardImagePath);
                    model.MovieCards[cardIndex].ImageUrl = "/images/Cards/" + Path.GetFileName(file.FileName);
                }
                cardIndex++;
            }

            var movieData = _mapper.Map<MovieTemplateInfE>(model);
            if(movieData.Country==null)movieData.Country = _movie.GetMovieInf(model.Id).Country;
            var result = _movie.UpdateMovieTemplate(movieData);
            if (result.Result)
            {
                return RedirectToAction("MovieTemplatePage",
                    "InformationSynchronization",
                    new { id = model.Id });
            }
            else
            {
                return RedirectToAction("Error404Page", "Error");
            }
        }
        
        [UserMod]
        [HttpPost]
        public ActionResult GetMoreFriendsRatings(int movieId)
        {
            SessionStatus();
            var moreRatings = _serviceFriend.GetFriendsMovieAll(movieId);
            var result = _mapper
                .Map<IEnumerable<ScoresFriendsGaveTheMovieModel>>(moreRatings)
                .Skip(5)
                .Select(r => 
                {
                    r.ReviewDateString = r.ReviewData.ToString("dd/MM/yyyy");
                    return r;
                });
            return Json(new { success = true, data = result });
        }
        
        [ModeratorMod]
        public ActionResult DeleteMovie(int? id)
        {
            SessionStatus();
            var exist = _movie.GetMovieInf(id);
            if (exist != null)
            {
                var category = exist.Category;
                MovieDeleteResult result = null;
                for (int i = 0; i < 10000; i++)
                {
                    result = _movie.DeleteMovie(i);
                }
                if (result != null && result.Result)
                {
                    switch (category)
                    {
                        case FilmCategory.Film:return RedirectToAction("MovieSearch","SearchSortAdd");
                        case FilmCategory.Cartoon:return RedirectToAction("CartoonsSearch","SearchSortAdd");
                        case FilmCategory.Anime:return RedirectToAction("AnimeSearch","SearchSortAdd");
                        case FilmCategory.Serial:return RedirectToAction("SerialsSearch","SearchSortAdd");
                        case FilmCategory.Non:return RedirectToAction("Novelty","SearchSortAdd");
                        default:return RedirectToAction("Error404Page","Error");
                    }
                }
            }
            return RedirectToAction("Error404Page", "Error");
        }
        #endregion
    }
}