using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.Movie.SearchPages;
using MovieNight.Web.Models.PersonalP;
using MovieNight.Web.Models.PersonalP.Bookmark;
using MovieNight.Web.Models.SortingSearchingFiltering;

namespace MovieNight.Web.Controllers
{
    public class SearchSortAddController : MasterController
    {
        
        private readonly IFriendsService _serviceFriend;
        private readonly IMapper _mapper;
        private readonly IMovie _movie;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static readonly object lockObj = new object();
        private static bool requestInProgress = false;
        private static Timer timer;
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


            });

            _mapper = config.CreateMapper();

        }
        // GET: SearchSortAdd
        [UserMod]
        public ActionResult FriendsPage(int _skipParametr)
        {
            var listU = _serviceFriend.getListOfFriends(_skipParametr);
            if (listU == null)
            {
                return View();
            }
            FriendListModel friendListModel = new FriendListModel();
            foreach (var t in listU.ListOfFriends)
            {
                FriendsPageD tmp = t; 
                var listOfFriends = _mapper.Map<FriendPageModel>(tmp);
                listOfFriends.BUserE = new UserModel
                {
                    Id = tmp.BUserE.Id,
                    Username = tmp.BUserE.Username,
                    Email = tmp.BUserE.Email
                };
                friendListModel.ListOfFriends.Add(listOfFriends);
            }
            return View(friendListModel);
        }

       

        

        public ActionResult SerialsSearch()
        {
            SessionStatus();
            int numP = 1;
            SessionStatus();
            if (System.Web.HttpContext.Current.GetListFilmS() != null)
            {
                if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Serial)
                    numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
            }
            if(TempData["MovieListModel"] == null){
                var filmsListModel = new MovieListModel
                {
                    CommandSort = new FilmCommandSort
                    {
                        PageNom = numP,
                        Category = FilmCategory.Serial,
                        Direction = System.Web.HttpContext.Current.GetListFilmS() == null ?  Direction.Non: System.Web.HttpContext.Current.GetListFilmS().CommandSort.Direction,
                        SortingDirection = System.Web.HttpContext.Current.GetListFilmS() == null ? SortDirection.Non : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortingDirection,
                        SortPar = System.Web.HttpContext.Current.GetListFilmS() == null ? SortingOption.All : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortPar,
                        UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                    }
                };
                var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
                var movieList = _movie.GetListMovie(filmSCommand);
                filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
                filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
                System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
                var partList = filmsListModel.ListFilm.Skip(
                    30 * numP - 1).Take(30).ToList();
                var listModel = new MovieListModel
                {
                    CommandSort = filmsListModel.CommandSort,
                    ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
                };
                return View(listModel);
            }
            var model = TempData["MovieListModel"] as MovieListModel;
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
                return View(model);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            return View(model);
        }
        public ActionResult CartoonsSearch()
        {
            SessionStatus();
            int numP = 1;
            SessionStatus();
            if (System.Web.HttpContext.Current.GetListFilmS() != null)
            {
                if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Cartoon)
                    numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
            }
            if(TempData["MovieListModel"] == null){
                var filmsListModel = new MovieListModel
                {
                    CommandSort = new FilmCommandSort
                    {
                        PageNom = numP,
                        Category = FilmCategory.Cartoon,
                        Direction = System.Web.HttpContext.Current.GetListFilmS() == null ?  Direction.Non: System.Web.HttpContext.Current.GetListFilmS().CommandSort.Direction,
                        SortingDirection = System.Web.HttpContext.Current.GetListFilmS() == null ? SortDirection.Non : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortingDirection,
                        SortPar = System.Web.HttpContext.Current.GetListFilmS() == null ? SortingOption.All : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortPar,
                        UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                    }
                };
                var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
                var movieList = _movie.GetListMovie(filmSCommand);
                filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
                filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
                System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
                var partList = filmsListModel.ListFilm.Skip(
                    30 * numP - 1).Take(30).ToList();
                var listModel = new MovieListModel
                {
                    CommandSort = filmsListModel.CommandSort,
                    ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
                };
                return View(listModel);
            }
            var model = TempData["MovieListModel"] as MovieListModel;
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
                return View(model);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            return View(model);
        }
        public ActionResult AnimeSearch()
        {
            SessionStatus();
            int numP = 1;
            SessionStatus();
            if (System.Web.HttpContext.Current.GetListFilmS() != null)
            {
                if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Anime)
                    numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
            }
            if(TempData["MovieListModel"] == null){
                var filmsListModel = new MovieListModel
                {
                    CommandSort = new FilmCommandSort
                    {
                        PageNom = numP,
                        Category = FilmCategory.Anime,
                        Direction = System.Web.HttpContext.Current.GetListFilmS() == null ?  Direction.Non: System.Web.HttpContext.Current.GetListFilmS().CommandSort.Direction,
                        SortingDirection = System.Web.HttpContext.Current.GetListFilmS() == null ? SortDirection.Non : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortingDirection,
                        SortPar = System.Web.HttpContext.Current.GetListFilmS() == null ? SortingOption.All : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortPar,
                        UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                    }
                };
                var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
                var movieList = _movie.GetListMovie(filmSCommand);
                filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
                filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
                System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
                var partList = filmsListModel.ListFilm.Skip(
                    30 * numP - 1).Take(30).ToList();
                var listModel = new MovieListModel
                {
                    CommandSort = filmsListModel.CommandSort,
                    ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
                };
                return View(listModel);
            }
            var model = TempData["MovieListModel"] as MovieListModel;
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
                return View(model);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            return View(model);
        }

        public async Task<ViewResult> RandomFilm()
        {
            SessionStatus();

            var randomMovie = await _movie.GetRandomFilm();
            
            var film = _mapper.Map<MovieTemplateInfModel>( randomMovie);
                
                
            return View(film);
        }

        [HttpPost]
        public async Task<JsonResult> GenRandomMovie()
        {
            SessionStatus();

            var randomMovie = await _movie.GetRandomFilm();
            var film = _mapper.Map<MovieTemplateInfModel>(randomMovie);
            return Json(new
            {
                success = true,
                title = film.Title,
                posterImage = Url.Content(film.PosterImage),
                productionYear = film.ProductionYear.ToString("dd/MM/yyyy"),
                movieGrade = film.MovieNightGrade,
                genre = film.Genre,
                bookmarked = film.Bookmark,
                bookmarkTimeOf = film.BookmarkTomeOf,
                
                
            });
        }
        
        
        [UserMod]
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
                    Id = tmp.BUserE.Id,
                    Username = tmp.BUserE.Username,
                    Email = tmp.BUserE.Email
                };
                friendListModel.ListOfFriends.Add(listOfUsers);
            }

            return View(friendListModel);
        }
        
        public ActionResult SetNewFriendPage(int? _friendId)
        {
            var _userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var _userVsFriend = _serviceFriend.setAddFriend((_userId, _friendId));
            if (_userVsFriend == true)
            {
                return RedirectToAction("FindFriends");
            }
            else
            {
                return RedirectToAction("Error404Page", "Error");
            }
        }
        public ActionResult SetDeleteFriendPage(int? _friendId)
        {
            var _userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var _userVsFriend = _serviceFriend.setDeleteFriend((_userId, _friendId));
            if (_userVsFriend == true)
            {
                return RedirectToAction("FriendsPage");
            }
            else
            {
                return RedirectToAction("Error404Page", "Error");
            }
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
            ViewBag.NumOfPage = 1;
            var viewingModel = _mapper.Map<List<ViewingHistoryModel>>(viewList);
            System.Web.HttpContext.Current.SetCommandViewList(new ViewListSort
            {
                PageNumber = ViewBag.CurrentPageNumber = 1,
                Field = SelectField.Non,
                DirectionStep = Direction.Non,
                SortingDirection = SortDirection.Non,
                SearchParameter = "",
                Category = FilmCategory.Non
            });

            var list = viewingModel.Take(10).ToList();
            System.Web.HttpContext.Current.SetListViewingHistoryS(viewingModel);
            
            return View(list);
        }

        #endregion
        
        [HttpPost]
//     public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
//     {
//         try
//         {
//             // if (Request.Form["cancel"] != null)
//             // {
//             //     CancelRequests();
//             //     return RedirectToAction("Cancelled");
//             // }
//             lock (lockObj)
//             {
//                 // Если запрос уже выполняется, не даем запустить новый
//                 if (requestInProgress)
//                 {
//                     return RedirectToAction("Cancelled");
//                 }
//
//                 // Устанавливаем флаг, что запрос начался
//                 requestInProgress = true;
//             }
//             
//             timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
//             // Выполняем запрос и ожидаем результат
//             var result = await PerformRequestAsync(command, cancellationTokenSource.Token);
//
//             // Возвращаем результат
//             return result;
//         }
//         finally
//         {
//             // После выполнения запроса снимаем флаг
//             lock (lockObj)
//             {
//                 requestInProgress = false;
//             }
//         }
//     }
//     private void TimerCallback(object state)
//     {
//         lock (lockObj)
//         {
//             // Сбрасываем флаг после истечения таймера
//             requestInProgress = false;
//         }
//     }
//     private async Task<ActionResult> PerformRequestAsync(ViewListSort command, CancellationToken cancellationToken)
//     {  cancellationToken.ThrowIfCancellationRequested();
//            if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
//         {
//             if (command.DirectionStep == Direction.Right)
//             {
//                 if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
//                     command.PageNumber += 1;
//     
//                 if (command.PageNumber <= 1) command.PageNumber = 1;
//                 else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
//                              ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
//                              : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
//                     command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
//                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
//                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
//             }
//             else if (command.DirectionStep == Direction.Left)
//             {
//                 if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
//                     command.PageNumber -= 1;
//     
//                 if (command.PageNumber <= 1) command.PageNumber = 1;
//                 else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
//                              ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
//                              : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
//                     command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
//                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
//                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
//             }
//     
//             var currStateListCache = System.Web.HttpContext.Current.GetListViewingHistoryS();
//             var currList = currStateListCache.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
//             System.Web.HttpContext.Current.SetCommandViewList(command);
//             ViewBag.CurrentPageNumber = command.PageNumber;
//     
//             return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
//         }else {
//                command.PageNumber = 1;
//                System.Web.HttpContext.Current.SetCommandViewList(command);
//                var transCommand = _mapper.Map<ViewListSortCommandE>(command);
//                var currStateList = await _movie.GetNewViewList(transCommand);
//                var newListV = _mapper.Map<List<ViewingHistoryModel>>(currStateList);
//                System.Web.HttpContext.Current.SetListViewingHistoryS(newListV);
//                var currList = newListV.Take(10).ToList();
//                ViewBag.CurrentPageNumber = command.PageNumber;
//                return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
//         }
//     }
//
// // Метод для отмены всех ожидающих запросов
//         public void CancelRequests()
//         {
//             cancellationTokenSource.Cancel();
//             // Сбрасываем таймер
//             timer?.Change(Timeout.Infinite, Timeout.Infinite);
//         }
//
//
// // Действие, которое будет вызываться, если запрос был отменен
//     public ActionResult Cancelled()
//     {
//         return View();
//     }
    // public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
    // {
    //     try
    //     {
    //         cancellationTokenSource.Token.ThrowIfCancellationRequested();
    //         cancellationTokenSource = new CancellationTokenSource();
    //         cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(0.005)); // Отмена через 5 секунд
    //
    //         // Делаем задержку на 5 секунд
    //         await Task.Delay(TimeSpan.FromSeconds(0.005), cancellationTokenSource.Token);
    //         
    //         
    //             if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
    //             {
    //                 if (command.DirectionStep == Direction.Right)
    //                 {
    //                     if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
    //                         command.PageNumber += 1;
    //
    //                     if (command.PageNumber <= 1) command.PageNumber = 1;
    //                     else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
    //                         command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                             ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                             : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
    //                 }
    //                 else if (command.DirectionStep == Direction.Left)
    //                 {
    //                     if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
    //                         command.PageNumber -= 1;
    //
    //                     if (command.PageNumber <= 1) command.PageNumber = 1;
    //                     else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
    //                         command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                             ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                             : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
    //                 }
    //
    //                 var currStateListCache = System.Web.HttpContext.Current.GetListViewingHistoryS();
    //                 var currList = currStateListCache.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
    //                 System.Web.HttpContext.Current.SetCommandViewList(command);
    //                 ViewBag.CurrentPageNumber = command.PageNumber;
    //
    //                 return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
    //             }
    //             else
    //             {
    //                 command.PageNumber = 1;
    //                 System.Web.HttpContext.Current.SetCommandViewList(command);
    //                 var transCommand = _mapper.Map<ViewListSortCommandE>(command);
    //                 var currStateList = await _movie.GetNewViewList(transCommand);
    //                 var newListV = _mapper.Map<List<ViewingHistoryModel>>(currStateList);
    //                 System.Web.HttpContext.Current.SetListViewingHistoryS(newListV);
    //                 var currList = newListV.Take(10).ToList();
    //                 ViewBag.CurrentPageNumber = command.PageNumber;
    //                 return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
    //             }
    //         
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         
    //         return RedirectToAction("Cancelled");
    //     }
    // }
    //
    // public void CancelRequests()
    // {
    //     cancellationTokenSource.Cancel(); 
    //     cancellationTokenSource.Dispose(); // Освобождаем ресурсы CancellationTokenSource
    //     cancellationTokenSource = new CancellationTokenSource(); // Создаем новый CancellationTokenSource для следующих запросов
    // }
    //
    // // Действие, которое будет вызываться, если запрос был отменен
    // public ActionResult Cancelled()
    // {
    //     return View();
    // }
    public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
    {
        
                if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
                {
                    if (command.DirectionStep == Direction.Right)
                    {
                        if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
                            command.PageNumber += 1;

                        if (System.Web.HttpContext.Current.GetCommandViewList().PageNumber > command.PageNumber)
                            command.PageNumber = (System.Web.HttpContext.Current.GetCommandViewList().PageNumber);
                        
                        
                        if (command.PageNumber <= 1) command.PageNumber = 1;
                        else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
                            ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                            : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
                            command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
                                ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                                : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
                    }
                    else if (command.DirectionStep == Direction.Left)
                    {
                        if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
                            command.PageNumber -= 1;
    
                        if (System.Web.HttpContext.Current.GetCommandViewList().PageNumber < command.PageNumber)
                            command.PageNumber = (System.Web.HttpContext.Current.GetCommandViewList().PageNumber);
                        
                        if (command.PageNumber <= 1) command.PageNumber = 1;
                        else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
                            ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                            : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
                            command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
                                ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
                                : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
                    }
    
                    var currStateListCache = System.Web.HttpContext.Current.GetListViewingHistoryS();
                    var currList = currStateListCache.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
                    System.Web.HttpContext.Current.SetCommandViewList(command);
                    ViewBag.CurrentPageNumber = command.PageNumber;
    
                    return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
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
                    ViewBag.CurrentPageNumber = command.PageNumber;
                    return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
                }
            
        
    }


    [HttpGet]
    public ActionResult MovieSearch()
    {
        SessionStatus();
            int numP = 1;
            SessionStatus();
            if (System.Web.HttpContext.Current.GetListFilmS() != null)
            {
                if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Film)
                    numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
            }
            if(TempData["MovieListModel"] == null){
                var filmsListModel = new MovieListModel
                {
                    CommandSort = new FilmCommandSort
                    {
                        PageNom = numP,
                        Category = FilmCategory.Film,
                        Direction = System.Web.HttpContext.Current.GetListFilmS() == null ?  Direction.Non: System.Web.HttpContext.Current.GetListFilmS().CommandSort.Direction,
                        SortingDirection = System.Web.HttpContext.Current.GetListFilmS() == null ? SortDirection.Non : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortingDirection,
                        SortPar = System.Web.HttpContext.Current.GetListFilmS() == null ? SortingOption.All : System.Web.HttpContext.Current.GetListFilmS().CommandSort.SortPar,
                        UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                    }
                };
                var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
                var movieList = _movie.GetListMovie(filmSCommand);
                filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
                filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
                System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
                var partList = filmsListModel.ListFilm.Skip(
                    30 * numP - 1).Take(30).ToList();
                var listModel = new MovieListModel
                {
                    CommandSort = filmsListModel.CommandSort,
                    ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
                };
                return View(listModel);
            }
            var model = TempData["MovieListModel"] as MovieListModel;
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
            {
                return View(model);
            }
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            return View(model);
    }

    [HttpPost]
    public ActionResult SortCurrFilm(FilmCommandSort command)
    {
        int numP = 1;
        SessionStatus();
        
        var filmsListModel = new MovieListModel
        {
            CommandSort = new FilmCommandSort
            {
                PageNom = numP,
                Category = command.Category,
                Direction = command.Direction,
                SortingDirection = command.SortingDirection,
                SortPar = command.SortPar,
                UserId = System.Web.HttpContext.Current.GetMySessionObject().Id

            }
        };
        var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
        var movieList = _movie.GetListMovie(filmSCommand);
        filmsListModel.CommandSort.MaxPage = movieList.Count/30;
        filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
        System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
        var partList = movieList.Take(30).ToList();
        var listModel = new MovieListModel
        {
            CommandSort = filmsListModel.CommandSort,
            ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
        };

        TempData["MovieListModel"] = listModel;


        switch (command.Category)
        {
            case FilmCategory.Anime:
                return RedirectToAction("AnimeSearch","SearchSortAdd");
            case FilmCategory.Cartoon:
                return RedirectToAction("CartoonsSearch","SearchSortAdd");
            case FilmCategory.Film:
                return RedirectToAction("MovieSearch","SearchSortAdd");
            case FilmCategory.Serial:
                return RedirectToAction("SerialsSearch","SearchSortAdd");
            case FilmCategory.Non:
                if(command.Direction == Direction.Novelty)
                    return RedirectToAction("Novelty","SearchSortAdd");
                if(command.Direction == Direction.ForYou)
                    return RedirectToAction("ForYou","SearchSortAdd");
                return RedirectToAction("Error404Page", "Error");
            default:
                return RedirectToAction("Error404Page", "Error");
        }
    }

    [HttpPost]
    public ActionResult SetNewPageFromNumber(int pageNumber)
    {

        var filmsListModel = System.Web.HttpContext.Current.GetListFilmS();
        filmsListModel.CommandSort.PageNom = pageNumber;
        System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
        var partList = filmsListModel.ListFilm.Skip(
            30 * pageNumber - 1).Take(30).ToList();
        var listModel = new MovieListModel
        {
            CommandSort = filmsListModel.CommandSort,
            ListFilm = partList
        };
        TempData["MovieListModel"] = listModel;

       // return RedirectToAction("MovieSearch","SearchSortAdd");
       return Json(listModel);

    }
    
    [HttpPost]
    public ActionResult LeftStepPage()
    {
        var filmsListModel = System.Web.HttpContext.Current.GetListFilmS();
        if(filmsListModel.CommandSort.PageNom-1>=1)filmsListModel.CommandSort.PageNom -= 1;
        System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
        var partList = filmsListModel.ListFilm.Skip(
            30 * filmsListModel.CommandSort.PageNom - 1).Take(30).ToList();
        var listModel = new MovieListModel
        {
            CommandSort = filmsListModel.CommandSort,
            ListFilm = partList
        };
        TempData["MovieListModel"] = listModel;

       // return RedirectToAction("MovieSearch","SearchSortAdd");
       return Json(listModel);


    }

    [HttpPost]
    public ActionResult RightStepPage()
    {
        var filmsListModel = System.Web.HttpContext.Current.GetListFilmS();
        if(filmsListModel.CommandSort.PageNom+1<filmsListModel.ListFilm.Count/30)filmsListModel.CommandSort.PageNom += 1;
        System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
        var partList = filmsListModel.ListFilm.Skip(
            30 * filmsListModel.CommandSort.PageNom - 1).Take(30).ToList();
        var listModel = new MovieListModel
        {
            CommandSort = filmsListModel.CommandSort,
            ListFilm = partList
        };
        TempData["MovieListModel"] = listModel;
        
        //return RedirectToAction("MovieSearch","SearchSortAdd");
        return Json(listModel);

    }

    public ActionResult Novelty()
    {
        int numP = 1;
        SessionStatus();
        if (System.Web.HttpContext.Current.GetListFilmS() != null)
        {
            if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Non)
                numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
        }
        if(TempData["MovieListModel"] == null){
            var filmsListModel = new MovieListModel
            {
                CommandSort = new FilmCommandSort
                {
                    PageNom = numP,
                    Category = FilmCategory.Non,
                    Direction = Direction.Non,
                    SortingDirection = SortDirection.Descending,
                    SortPar = SortingOption.ReleaseDate,
                    UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                }
            };
            var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
            var movieList = _movie.GetListMovie(filmSCommand);
            filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
            filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
            System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
            var partList = filmsListModel.ListFilm.Skip(
                30 * numP - 1).Take(30).ToList();
            var listModel = new MovieListModel
            {
                CommandSort = filmsListModel.CommandSort,
                ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
            };
            return View(listModel);
        }
        var model = TempData["MovieListModel"] as MovieListModel;
        if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
        {
            return View(model);
        }
        if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
        {
            return RedirectToAction("Login", "Identification");
        }
        return View(model);
    }

    public ActionResult ForYou()
    {
        int numP = 1;
        SessionStatus();
        if (System.Web.HttpContext.Current.GetListFilmS() != null)
        {
            if(System.Web.HttpContext.Current.GetListFilmS().CommandSort.Category == FilmCategory.Non)
                numP = System.Web.HttpContext.Current.GetListFilmS().CommandSort.PageNom;
        }
        if(TempData["MovieListModel"] == null){
            var filmsListModel = new MovieListModel
            {
                CommandSort = new FilmCommandSort
                {
                    PageNom = numP,
                    Category = FilmCategory.Non,
                    Direction = Direction.ForYou,
                    SortingDirection = SortDirection.Descending,
                    SortPar = SortingOption.ReleaseDate,
                    UserId = System.Web.HttpContext.Current.GetMySessionObject()?.Id ?? null

                }
            };
            var filmSCommand = _mapper.Map<MovieCommandS>(filmsListModel.CommandSort);
            var movieList = _movie.GetListMovie(filmSCommand);
            filmsListModel.CommandSort.MaxPage = movieList.Count / 30;
            filmsListModel.ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(movieList);
            System.Web.HttpContext.Current.SetListFilmS(filmsListModel);
            var partList = filmsListModel.ListFilm.Skip(
                30 * numP - 1).Take(30).ToList();
            var listModel = new MovieListModel
            {
                CommandSort = filmsListModel.CommandSort,
                ListFilm = _mapper.Map<List<MovieTemplateInfModel>>(partList)
            };
            return View(listModel);
        }
        var model = TempData["MovieListModel"] as MovieListModel;
        if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] == "zero")
        {
            return View(model);
        }
        if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
        {
            return RedirectToAction("Login", "Identification");
        }
        return View(model);
    }
    
    
    
    
    #region Bookmark Page

    
    // public async Task<ActionResult> CurrentSortingAndFilteringActionBookmarkTomeOf(ViewListSort command)
    // {
    //     
    //             if (HttpContextInfrastructure.CurrentCommandStateComparison(command))
    //             {
    //                 if (command.DirectionStep == Direction.Right)
    //                 {
    //                     if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
    //                         command.PageNumber += 1;
    //
    //                     if (System.Web.HttpContext.Current.GetCommandViewList().PageNumber > command.PageNumber)
    //                         command.PageNumber = (System.Web.HttpContext.Current.GetCommandViewList().PageNumber);
    //                     
    //                     
    //                     if (command.PageNumber <= 1) command.PageNumber = 1;
    //                     else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
    //                         command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                             ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                             : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
    //                 }
    //                 else if (command.DirectionStep == Direction.Left)
    //                 {
    //                     if ((System.Web.HttpContext.Current.GetCommandViewList().PageNumber == command.PageNumber))
    //                         command.PageNumber -= 1;
    //
    //                     if (System.Web.HttpContext.Current.GetCommandViewList().PageNumber < command.PageNumber)
    //                         command.PageNumber = (System.Web.HttpContext.Current.GetCommandViewList().PageNumber);
    //                     
    //                     if (command.PageNumber <= 1) command.PageNumber = 1;
    //                     else if (command.PageNumber >= (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                         ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                         : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1))
    //                         command.PageNumber = (((System.Web.HttpContext.Current.GetListViewingHistoryS().Count % 10) == 0)
    //                             ? (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10)
    //                             : (System.Web.HttpContext.Current.GetListViewingHistoryS().Count / 10) + 1);
    //                 }
    //
    //                 var currStateListCache = System.Web.HttpContext.Current.GetListViewingHistoryS();
    //                 var currList = currStateListCache.Skip((command.PageNumber - 1) * 10).Take(10).ToList();
    //                 System.Web.HttpContext.Current.SetCommandViewList(command);
    //                 ViewBag.CurrentPageNumber = command.PageNumber;
    //
    //                 return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
    //             }
    //             else
    //             {
    //                 command.PageNumber = 1;
    //                 System.Web.HttpContext.Current.SetCommandViewList(command);
    //                 var transCommand = _mapper.Map<ViewListSortCommandE>(command);
    //                 var currStateList = await _movie.GetNewBookmarkTimeOfList(transCommand);
    //                 var newListV = _mapper.Map<List<ViewingHistoryModel>>(currStateList);
    //                 System.Web.HttpContext.Current.SetListViewingHistoryS(newListV);
    //                 var currList = newListV.Take(10).ToList();
    //                 ViewBag.CurrentPageNumber = command.PageNumber;
    //                 return Json(new { success = true, newListV = currList, pageNumber = command.PageNumber });
    //             }
    //         
    //     
    // }

    [HttpGet]
    public ActionResult BookmarkTimeOfPage()
    {
        var bookmarkList = _movie.GetListBookmarksTimeOfInfo(System.Web.HttpContext.Current.GetMySessionObject().Id);
        var bookmarkModel = _mapper.Map<List<BookmarkPageInfoTableModel>>(bookmarkList);
        System.Web.HttpContext.Current.SetListToSession(bookmarkModel);
        ViewBag.CurrentPageNumber = 1;

        var list = bookmarkModel.Take(10).ToList();
        return View(list);
    }
    
    [HttpPost]
    public async Task<ActionResult> BookmarkTimeOfSortingAndFilteringAction(ListSortCommand command)
    {
        command.UserId = System.Web.HttpContext.Current.GetMySessionObject().Id;
        return await SortingAndFilteringCommand(
            command,
            () => _movie.GetNewBookmarkTimeOfList(_mapper.Map<ListSortCommandE>(command)),
            entities => _mapper.Map<List<BookmarkPageInfoTableModel>>(entities));
    }
    
    // [HttpGet]
    // public ActionResult ViewedList()
    // {
    //     var viewList = _movieService.GetViewingList(HttpContext.Current.GetMySessionObject().Id);
    //     var viewingModel = _mapper.Map<List<ViewingHistoryModel>>(viewList);
    //     HttpContext.Current.SetListToSession(viewingModel);
    //     ViewBag.NumOfPage = 1;
    //
    //     var list = viewingModel.Take(10).ToList();
    //     return View(list);
    // }
    //
    // [HttpPost]
    // public async Task<ActionResult> CurrentSortingAndFilteringAction(ViewListSort command)
    // {
    //     return await HandleSortingAndFiltering(
    //         command,
    //         () => _movieService.GetNewViewList(_mapper.Map<ViewListSortCommandE>(command)),
    //         entities => _mapper.Map<List<ViewingHistoryModel>>(entities));
    // }
    
    
    public ActionResult BookmarkPage()
    {
        return View();
    }


    #endregion
    
    
    
   
    
    
    


        
    }
}
