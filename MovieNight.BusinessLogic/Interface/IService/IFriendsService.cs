﻿using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IFriendsService 
    {
        FriendsPageD getFriendDate(int? id);
        FriendsListD getListOfUsers(int _skipParameter, string _searchParameter);
        FriendsListD getListOfFriends(int _skipParameter, string _searchParameter);
        bool setAddFriend((int _userId, int? _friendId) valueTuple);
        bool setDeleteFriend((int _userId, int? _friendId) valueTuple);
        int GetTotalUserCount(string _searchParameter);
        int GetTotalFriendsCount(string _searchParameter);
    }
}
