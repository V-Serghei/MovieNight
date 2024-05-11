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
        List<InboxD> InboxEquipment(int? userId);
        bool SetAddMessage(InboxD message);
        List<InboxD> InboxSent(int? userId);
        List<InboxD> InboxStarred(int? userId);
        InboxD InboxRead(int? mailId);
        bool SetMailStar(int? mailId);
        bool DeleteMailStar(int? mailId);
        bool DeleteMail(int? mailId);
    }
}
