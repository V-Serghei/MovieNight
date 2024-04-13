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
    }
}
