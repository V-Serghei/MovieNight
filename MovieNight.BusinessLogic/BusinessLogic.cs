using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IMail;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.BusinessLogic.Session;
using MovieNight.BusinessLogic.Session.MailS;
using MovieNight.BusinessLogic.Session.Service;

namespace MovieNight.BusinessLogic
{
    public class BusinessLogic
    {
        public ISession Session()
        {
            return new SessionBL();
        }
        public IInbox GetInbox()
        {
            return new InboxS();
        }

        public IMovie GetMovieService()
        {
            return new MovieService();
        }
        
        public IFriendsService GetFriendsService()
        {
            return new FriendService();
        }

        public IAchievements GetAchievementsService()
        {
            return new AchievementsService();
        }
    }
}
