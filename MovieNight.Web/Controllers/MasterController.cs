using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.PersonalP.Bookmark;
using MovieNight.Web.Models.SortingSearchingFiltering;

namespace MovieNight.Web.Controllers
{
    public class MasterController : Controller
    {
        #region general settings
        /// <summary>
        ///  are used throughout the controller area
        /// </summary>
        
        private readonly ISession _session;
        private readonly IMapper _mapper;
        private readonly IMovie _movie;
        public MasterController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LogInData,UserModel >();
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
                cfg.CreateMap<BookmarkE, BookmarkModel>();
                cfg.CreateMap<BookmarkModel, BookmarkE>();


                cfg.CreateMap<FriendsPageD, FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore());
            });
            
            _mapper = config.CreateMapper();
            var bl = new BusinessLogic.BusinessLogic();
            _session = bl.Session();
            _movie = bl.GetMovieService();
        }

        #endregion

        #region session

        public void SessionStatus()
        {
            var apiCookie = Request.Cookies["X-KEY"];
            _session.CleanupExpiredSessionRange();
            
            if (apiCookie != null)
            {
                var profile = _session
                    .GetUserByCookie(apiCookie.Value,HttpContextInfrastructure.GetUserAgentInfo(Request));
                
                if (profile != null)
                {                
                    var us = _mapper.Map<UserModel>(profile);
                    System.Web.HttpContext.Current.SetMySessionObject(us);
                    System.Web.HttpContext.Current.Session["LoginStatus"] = "login";
                    System.Web.HttpContext.Current.Session["UserId"] = us.Id;
                    GetInfoOnTheCurrStateBookmarkTimeOf();
                }
                else
                {
                    System.Web.HttpContext.Current.Session.Clear();
                    if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("X-KEY"))
                    {
                        var cookie = ControllerContext.HttpContext.Request.Cookies["X-KEY"];
                        if (cookie != null)
                        {
                            cookie.Expires = DateTime.Now.AddDays(-1);
                            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                        }
                    }
                    System.Web.HttpContext.Current.Session.Clear();
                    
                    System.Web.HttpContext.Current.Session["LoginStatus"] = "logout";
                }
            }
            else
            {
                System.Web.HttpContext.Current.Session.Clear();
                System.Web.HttpContext.Current.Session["LoginStatus"] = "zero";
            }
        }

        #endregion
        
        #region transition tools
        protected async Task<ActionResult> SortingAndFilteringCommand<TModel, TEntity>(
            ListSortCommand command,
            Func<Task<List<TEntity>>> getNewList,
            Func<List<TEntity>, List<TModel>> mapToModel)
        {
            var currentStateList = System.Web.HttpContext.Current.GetListFromSession<TModel>();

            if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
            {
                if (command.DirectionStep == Direction.Right)
                {
                    command.PageNumber = Math.Min(command.PageNumber + 1, GetTotalPages(currentStateList));
                }
                else if (command.DirectionStep == Direction.Left)
                {
                    command.PageNumber = Math.Max(command.PageNumber - 1, 1);
                }

                var currentList = currentStateList.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
                System.Web.HttpContext.Current.SetCommandState(command);
                ViewBag.CurrentPageNumber = command.PageNumber;

                return Json(new { success = true, newListV = currentList, pageNumber = command.PageNumber });
            }
            else
            {
                command.PageNumber = 1;
                System.Web.HttpContext.Current.SetCommandState(command);

                var newStateList = await getNewList();
                var newModelList = mapToModel(newStateList);
                System.Web.HttpContext.Current.SetListToSession(newModelList);

                var currentList = newModelList.Take(10).ToList();
                ViewBag.CurrentPageNumber = command.PageNumber;

                return Json(new { success = true, newListV = currentList, pageNumber = command.PageNumber });
            }
        }

        private int GetTotalPages<T>(List<T> list)
        {
            return (list.Count + 9) / 10;
        }
        
        
        #endregion
        
        #region Temporary Data
        /// <summary>
        /// Update and obtain information on temporary bookmarks
        /// </summary>

        public void GetInfoOnTheCurrStateBookmarkTimeOf()
        {
            if (System.Web.HttpContext.Current.GetBookmarkTimeOf() == null)
            {
                var listBookmarkE =
                    _movie.GetListBookmarksTimeOf(System.Web.HttpContext.Current.GetMySessionObject().Id);
                var listBookmark = new BookmarkTimeOf
                {
                    Bookmark = _mapper.Map<List<BookmarkModel>>(listBookmarkE.Bookmark),
                    MovieInTimeOfBookmark = _mapper
                        .Map<List<MovieTemplateInfModel>>(listBookmarkE.MovieInTimeOfBookmark)
                };
                System.Web.HttpContext.Current.SetBookmarkTimeOf(listBookmark);
            }

            _movie.BookmarkStatusCheck();
        }
        

        #endregion

    }
}