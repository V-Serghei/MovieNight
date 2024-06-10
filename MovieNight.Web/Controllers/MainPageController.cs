using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.DifferentE;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models.DifModel;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.PersonalP.Bookmark;
using MovieNight.Web.Models.SortingSearchingFiltering;

namespace MovieNight.Web.Controllers
{
    public class MainPageController : MasterController
    {
        #region Basic Settings
        private readonly ISession _sessionUser;
        private readonly IMapper _mapper;
        private readonly IMovie _movie;
        

        public MainPageController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();
            _movie = sesControlBl.GetMovieService();

              var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<FriendsPageD , FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt => opt.Ignore())
                    .ForMember(dest=>dest.ViewingHistory, 
                        opt=>opt.Ignore())
                    .ForMember(dest=>dest.ListInThePlans, 
                        opt=>opt.Ignore());
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
                cfg.CreateMap<ViewingHistoryM, ViewingHistoryModel>()
                    .ForPath(dest => dest.YearOfRelease, opt =>
                        opt.MapFrom(src => src.YearOfRelease.ToString("yyyy-MM-dd")))
                    .ForPath(dest => dest.ReviewDate, opt =>
                        opt.MapFrom(src => src.ReviewDate.ToString("yyyy-MM-dd")));
                cfg.CreateMap<ViewingHistoryModel,ViewingHistoryM>();
                cfg.CreateMap<ViewListSort, ViewListSortCommandE>();
                cfg.CreateMap<ViewListSortCommandE,ViewListSort>();
                cfg.CreateMap<FilmCommandSort, MovieCommandS>();
                cfg.CreateMap<MovieCommandS, FilmCommandSort>();
                cfg.CreateMap<ListSortCommandE, ListSortCommand>();
                cfg.CreateMap<ListSortCommand, ListSortCommandE>();
                cfg.CreateMap<BookmarkPageInfoTableModel, BookmarkInfoE>()
                    .ForMember(dest => dest.YearOfRelease, opt => opt.MapFrom(src => DateTime.Parse(src.YearOfRelease)))
                    .ForMember(dest => dest.BookmarkDate, opt => opt.MapFrom(src => DateTime.Parse(src.BookmarkDate)));

                cfg.CreateMap<BookmarkInfoE, BookmarkPageInfoTableModel>()
                    .ForMember(dest => dest.YearOfRelease, opt => opt.MapFrom(src => src.YearOfRelease.ToString("yyyy-MM-dd")))
                    .ForMember(dest => dest.BookmarkDate, opt => opt.MapFrom(src => src.BookmarkDate.ToString("g")));
                cfg.CreateMap<AreWatchingModel, AreWatchingE>();
                cfg.CreateMap<AreWatchingE, AreWatchingModel>();

                cfg.CreateMap<AreWatchingModel, MovieTemplateInfE>();
                cfg.CreateMap<MovieTemplateInfE, AreWatchingModel>();

                cfg.CreateMap<TopFilmsModel, TopFilmsE>();
                cfg.CreateMap<TopFilmsE , TopFilmsModel>();
              });

            _mapper = config.CreateMapper();
            
        }
        #endregion
        
        #region Home Page
        [GuestMod]
        [HttpGet]
        public ActionResult Index()
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
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            if(user==null){
                HttpContextInfrastructure.SerGlobalParam(_sessionUser.GetIdCurrUser(null));
            }
            else
            {
                HttpContextInfrastructure.SerGlobalParam(_sessionUser.GetIdCurrUser(user.Username));
            }
            return View();
        }
        [GuestMod]
        [HttpGet]
        public ActionResult Primary()
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
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            if(user==null){HttpContextInfrastructure.SerGlobalParam(_sessionUser.GetIdCurrUser(null));
            }
            else
            {
                HttpContextInfrastructure.SerGlobalParam(_sessionUser.GetIdCurrUser(user.Username));
            }
            
            return View();
        }
        [GuestMod]
        [HttpGet]
        public ActionResult News()
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
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            return View();
        }
        [GuestMod]
        [HttpGet]
        public ActionResult Top()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
                var listMovie = _movie.GetMoviesTop(null);
                var listModel = _mapper.Map<List<TopFilmsModel>>(listMovie);
                return View(listModel);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            var listMovieExUser = _movie.GetMoviesTop(user.Id);
            var listModelExUser = _mapper.Map<List<TopFilmsModel>>(listMovieExUser);
            
            return View(listModelExUser);
        }
        [GuestMod]
        [HttpGet]
        public ActionResult AreWatching()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
               var listMovie = _movie.GetMoviesAreWatching(null);
               var listModel = _mapper.Map<List<AreWatchingModel>>(listMovie);
               return View(listModel);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            var listMovieExUser = _movie.GetMoviesAreWatching(user.Id);
            var listModelExUser = _mapper.Map<List<AreWatchingModel>>(listMovieExUser);
            
            return View(listModelExUser);
            
        }
        #endregion
    }
}