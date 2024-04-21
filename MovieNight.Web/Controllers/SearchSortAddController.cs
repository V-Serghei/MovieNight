using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Friends;
using MovieNight.Web.Models.PersonalP;

namespace MovieNight.Web.Controllers
{
    public class SearchSortAddController : Controller
    {
        private readonly IFriendsService _serviceFriend;
        private readonly IMapper _mapper;
        public SearchSortAddController()
        {
            var serviceFriend = new BusinessLogic.BusinessLogic();
            _serviceFriend = serviceFriend.GetFriendsService();
            
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<FriendsPageD , FriendPageModel>()
                    .ForMember(dest=>dest.BUserE, 
                        opt=>opt.Ignore())
                    .ForMember(dest=>dest.ViewingHistory, 
                        opt=>opt.Ignore())
                    .ForMember(dest=>dest.ListInThePlans, 
                        opt=>opt.Ignore());
                
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

        public ActionResult ViewedList()
        {
            return View();
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
    }
}