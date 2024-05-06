using Microsoft.Win32;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;

namespace MovieNight.BusinessLogic.Core
{
    public class MailCore
    {
        public List<InboxD> InboxEquipmentFromData ()
        {
            var config = new MapperConfiguration(c =>
            {
                // c.CreateMap<>()
                //     .ForMember(dest => dest.BUserE, 
                //         opt => opt.Ignore());
            });
            
            var mapper = config.CreateMapper();
            //find user by ID
            List<InboxD> inboxDs = new List<InboxD> ();
            using (var db = new UserContext())
            {
                try
                {
                    inboxDs.Add(new InboxD
                        {
                            IsChecked = true,
                            //SenderName = user.Username,
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
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
            return inboxDs;
        }
    }
}
