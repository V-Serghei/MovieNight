using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class FriendService : FriendAPI, IFriendsService
    {
        public FriendsPageD getFriendDate(int? id)
        {
            return getFriendDateDB(id);
        }

        public FriendsListD getListOfUsers( int _skipParameter)
        {
            return getListOfUsersD(_skipParameter);
        }
        public FriendsListD getListOfFriends(int _skipParameter)
        {
            return getListOfFriendsD(_skipParameter);
        }

        public bool setAddFriend((int _userId, int? _friendId) valueTuple)
        {
            return setAddFriendD(valueTuple);
        }
        
        public bool setDeleteFriend((int _userId, int? _friendId) valueTuple)
        {
            return setDeleteFriendD(valueTuple);
        }

        
    }
}
