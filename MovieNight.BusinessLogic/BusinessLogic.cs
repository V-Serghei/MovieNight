using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Interface.IMail;
using MovieNight.BusinessLogic.Session;
using MovieNight.BusinessLogic.Session.MailS;

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
        
    }
}
