using MovieNight.BusinessLogic.Core;
using MovieNight.BusinessLogic.Interface.IMail;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Session.MailS
{
    public class InboxS : MailCore, IInbox
    {
        public List<InboxD> InboxEquipment(int? userId)
        {
            return InboxEquipmentFromData(userId);
        }

        public bool SetAddMessage(InboxD message)
        {
            return SetAddMessageDb(message);
        }
        public List<InboxD> InboxSent(int? userId)
        {
            return InboxSentFromData(userId);
        }
        public List<InboxD> InboxStarred(int? userId)
        {
            return InboxStarredFromData(userId);
        }
    }
}
