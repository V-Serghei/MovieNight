using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
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
                cfg.CreateMap<ViewingHistoryM, ViewingHistoryModel>()
                    .ForPath(dest => dest.YearOfRelease, opt =>
                        opt.MapFrom(src => src.YearOfRelease.ToString("yyyy-MM-dd")))
                    .ForPath(dest => dest.ReviewDate, opt =>
                        opt.MapFrom(src => src.ReviewDate.ToString("yyyy-MM-dd")));
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
            
            var list = viewingModel.Take(10).ToList();
            System.Web.HttpContext.Current.SetListViewingHistoryS(viewingModel);
            
            return View(list);
        }

        #endregion
        
        [HttpPost]
        public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
        {
            if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
            {
                if (command.DirectionStep == Direction.Right)
                {
                    command.PageNumber += 1;
                    if (command.PageNumber <= 1) command.PageNumber = 1;
                    else if (command.PageNumber >= System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                        command.PageNumber = System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10;
                }
                else if(command.DirectionStep == Direction.Left) 
                {
                    command.PageNumber -= 1;

                    if (command.PageNumber <= 1) command.PageNumber = 1;
                    else if (command.PageNumber >= System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                        command.PageNumber = System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10;
                }
                var currStateListCache = System.Web.HttpContext.Current.GetListViewingHistoryS();
                var currList = currStateListCache.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
                ViewBag.NumOfPage = command.PageNumber;

                return Json(new { success = true, newListV = currList });
            }
            else
            {
                command.PageNumber = 1;
                System.Web.HttpContext.Current.SetCommandViewList(command);
                var transCommand = _mapper.Map<ViewListSortCommandE>(command);
                var currStateList = await _movie.GetNewViewList(transCommand);
                var newListV = _mapper.Map<List<ViewingHistoryModel>>(currStateList);
                System.Web.HttpContext.Current.SetListViewingHistoryS(newListV);
                var currList = newListV.Take(10).ToList();
                ViewBag.NumOfPage = command.PageNumber;
                return Json(new { success = true, newListV = currList });

            }
            
            //var currStateList = await _movie.GetNewViewList(transCommand);
            //var newListV = _mapper.Map<List<ViewingHistoryModel>>(currStateList);

            // return Json(new { success = true, newListV = newListV });
        }

        
        
    }
}
