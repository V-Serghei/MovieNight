using MovieNight.BusinessLogic.Session.Service;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class FriendAPI
    {
        public FriendsPageD getFriendDateDB(int? id)
        {
            FriendsPageD friendsPageD = new FriendsPageD();
            using (var db = new UserContext())
            {
                try
                {
                    var person = db.UsersT.FirstOrDefault(p => p.Id == id);
                    if (person != null)
                    {
                        var config = new MapperConfiguration(c =>
                        {
                            c.CreateMap<PEdBdTable,FriendsPageD>()
                                .ForMember(dest => dest.BUserE, 
                                    opt => opt.Ignore());
                        });

                        var mapper = config.CreateMapper();
                        using (var persondb = new UserContext())
                        {
                            var search = persondb.PEdBdTables.FirstOrDefault(s =>
                                s.UserDbTableId == person.Id);
                            if (search != null)
                            {
                                mapper.Map(search, friendsPageD);
                            }

                            friendsPageD.BUserE = new UserE
                            {
                                Username = person.UserName,
                                Email = person.Email
                            };
                        }
                    }
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
            return friendsPageD;
        }
    }
}
