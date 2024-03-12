using Microsoft.Win32;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.Core
{
    public class MailCore
    {
        public List<InboxD> InboxEquipmentFromData (UserE user)
        {
            //find user by ID
            List<InboxD> inboxDs = new List<InboxD> ();
            inboxDs.Add(new InboxD
            {
                IsChecked = true,
                SenderName = user.Name,
                Theme = "I'm first",
                Message = "My first message",
                Date = new TimeD
                {
                    Year = 2024,
                    Month = 03,
                    Day = 05,
                },
            }
            );
            inboxDs.Add(new InboxD
            {
                IsChecked = false,
                SenderName = "Pupsic",
                Theme = "I'm secand",
                Message = "My secand message",
                Date = new TimeD
                {
                    Year = 2024,
                    Month = 03,
                    Day = 10,
                },
            }
            );
            return inboxDs;
        }
    }
}
