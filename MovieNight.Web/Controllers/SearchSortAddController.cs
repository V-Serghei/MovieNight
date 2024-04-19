using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Misc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.SortingSearchingFiltering;

namespace MovieNight.Web.Controllers
{
    public class SearchSortAddController : Controller
    {
        private readonly IFriendsService _serviceFriend;
        private readonly IMapper _mapper;
        private readonly IMovie _movie;
        public SearchSortAddController()
        {
            var service = new BusinessLogic.BusinessLogic();
            _serviceFriend = service.GetFriendsService();
            _movie = service.GetMovieService();
            
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<FriendsPageD , FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore())
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
                cfg.CreateMap<ViewingHistoryM,ViewingHistoryModel>();
                cfg.CreateMap<ViewingHistoryModel,ViewingHistoryM>();
                cfg.CreateMap<ViewListSort, ViewListSortCommandE>();
                cfg.CreateMap<ViewListSortCommandE,ViewListSort>();
                

            });

            _mapper = config.CreateMapper();

        }
        // GET: SearchSortAdd
        public ActionResult FriendsPage()
        {
            var listU = _serviceFriend.getListOfFriends();
            FriendListModel friendListModel = new FriendListModel();
            foreach (var t in listU.ListOfFriends)
            {
                FriendsPageD tmp = t; 
                var listOfUsers = _mapper.Map<FriendPageModel>(tmp);
                listOfUsers.BUserE = new UserModel
                {
                    Username = tmp.BUserE.Username,
                    Email = tmp.BUserE.Email
                };
                friendListModel.ListOfFriends.Add(listOfUsers);
            }

            return View(friendListModel);
        }

       

        public ActionResult MovieSearch()
        {
            return View();
        }

        public ActionResult SerialsSearch()
        {
            return View();
        }
        public ActionResult CartoonsSearch()
        {
            return View();
        }
        public ActionResult AnimeSearch()
        {
            return View();
        }

        public ActionResult RandomFilm()
        {
            return View();
        }
        public ActionResult FriendsMovies()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult FindFriends(int _skipParametr)
        {
            var listU = _serviceFriend.getListOfUsers(_skipParametr);
            FriendListModel friendListModel = new FriendListModel();
            if (listU == null)
            {
                return View();
            }
            foreach (var t in listU.ListOfFriends)
            {
                FriendsPageD tmp = t; 
                var listOfUsers = _mapper.Map<FriendPageModel>(tmp);
                listOfUsers.BUserE = new UserModel
                {
                    Username = tmp.BUserE.Username,
                    Email = tmp.BUserE.Email
                };
                friendListModel.ListOfFriends.Add(listOfUsers);
            }

            return View(friendListModel);
        }


        #region ViewedList

        /// <summary>
        /// Viewed List
        /// management and processing
        /// </summary>

        [HttpGet]
        public ActionResult ViewedList()
        {
            var viewList = _movie.GetViewingList(System.Web.HttpContext.Current.GetMySessionObject().Id);
            var viewingModel = _mapper.Map<List<ViewingHistoryModel>>(viewList);
            return View(viewingModel);
        }

        #endregion
        
        [HttpPost]
        public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
        {
            var transCommand = _mapper.Map<ViewListSortCommandE>(command);
            transCommand.Category = FilmCategory.Film;
            transCommand.SearchParameter = "G";
            
            var currStateList = await _movie.GetNewViewList(transCommand);
            
            List<ViewingHistoryModel> newListV = new ListStack<ViewingHistoryModel>();
            return Json(new {newListV });
        }
        
        
    }
}
