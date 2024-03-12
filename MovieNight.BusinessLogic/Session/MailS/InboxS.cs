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
        public List<InboxD> InboxEquipment(UserE user)
        {
            return InboxEquipmentFromData(user);
        }
    }
}
