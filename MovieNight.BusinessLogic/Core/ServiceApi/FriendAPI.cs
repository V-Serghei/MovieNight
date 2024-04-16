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
        
        public FriendsListD getListOfUsersD(int _skipParameter)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<PEdBdTable,FriendsPageD>()
                    .ForMember(dest => dest.BUserE, 
                        opt => opt.Ignore());
            });
            
            var mapper = config.CreateMapper();
            
            FriendsListD friendsListD = new FriendsListD();
            using (var db = new UserContext())
            {
                try
                {
                    var list9Users = db.UsersT.OrderBy(u => u.Id).Skip(_skipParameter*9).Take(9).ToList();
                    foreach (var list9 in list9Users)
                    {
                        var userd = db.PEdBdTables.FirstOrDefault(p => p.UserDbTableId == list9.Id);
                        if (userd != null)
                        {
                            var oneOfList = mapper.Map<FriendsPageD>(userd);
                            oneOfList.BUserE = new UserE
                            {
                                Username = list9.UserName,
                                Email = list9.Email
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                        else
                        {
                            var oneOfList = new FriendsPageD
                            {
                                BUserE = new UserE
                                {
                                    Username = list9.UserName,
                                    Email = list9.Email
                                }
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                    } 
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
            return friendsListD;
        }
        
        public FriendsListD getListOfFriendsD()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<PEdBdTable,FriendsPageD>()
                    .ForMember(dest => dest.BUserE, 
                        opt => opt.Ignore());
            });
            
            var mapper = config.CreateMapper();
            
            FriendsListD friendsListD = new FriendsListD();
            using (var db = new UserContext())
            {
                try
                {
                    var list10Users = db.UsersT.Take(10).ToList();
                    foreach (var list10 in list10Users)
                    {
                        var userd = db.PEdBdTables.FirstOrDefault(p => p.UserDbTableId == list10.Id);
                        if (userd != null)
                        {
                            var oneOfList = mapper.Map<FriendsPageD>(userd);
                            oneOfList.BUserE = new UserE
                            {
                                Username = list10.UserName,
                                Email = list10.Email
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                        else
                        {
                            var oneOfList = new FriendsPageD
                            {
                                BUserE = new UserE
                                {
                                    Username = list10.UserName,
                                    Email = list10.Email
                                }
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                    } 
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
            return friendsListD;
        }
    }
    
}
