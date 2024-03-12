using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Interface.IMail
{
    public interface IInbox
    {
        List<InboxD> InboxEquipment(UserE user);
    }
}
