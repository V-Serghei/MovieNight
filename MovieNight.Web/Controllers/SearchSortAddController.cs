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
    // // Метод для отмены всех ожидающих запросов
    // public void CancelRequests()
    // {
    //     cancellationTokenSource.Cancel(); // Отменяем токен
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
    


        
    }
}
