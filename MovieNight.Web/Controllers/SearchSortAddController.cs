using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.Friends;
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
        public ActionResult FriendsPage()
        {
            var listU = _serviceFriend.getListOfUsers();
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
        public ActionResult FriendsMovies()
        {
            return View();
        }
    }
}