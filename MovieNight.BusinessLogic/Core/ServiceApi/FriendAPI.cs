using MovieNight.BusinessLogic.Session.Service;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                    var userId = HttpContext.Current.Session["UserId"] as int?;
                    var list9Users = db.UsersT.Where(u =>u.Id != (int)userId).ToList();
                    foreach (var list9 in list9Users)
                    {
                        var userd = db.PEdBdTables.FirstOrDefault(p => p.UserDbTableId == list9.Id);
                        var existsInFriendsDb = db.Friends.FirstOrDefault(f => f.IdFriend == list9.Id && f.IdUser == userId);
                        if (userd != null && existsInFriendsDb == null)
                        {
                            var oneOfList = mapper.Map<FriendsPageD>(userd);
                            oneOfList.BUserE = new UserE
                            {
                                Id = list9.Id,
                                Username = list9.UserName,
                                Email = list9.Email
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                        else if(existsInFriendsDb == null)
                        {
                            var oneOfList = new FriendsPageD
                            {
                                BUserE = new UserE
                                {
                                    Id = list9.Id,
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
            friendsListD.ListOfFriends = friendsListD.ListOfFriends.Skip(_skipParameter*9).Take(9).ToList();
            return friendsListD;
        }
        public FriendsListD getListOfFriendsD(int _skipParameter)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<PEdBdTable,FriendsPageD>()
                    .ForMember(dest => dest.BUserE, 
                        opt => opt.Ignore());
                c.CreateMap<FriendsPageD,PEdBdTable>()
                    .ForMember(dest => dest.UserDbTableId, 
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
                        var userId = (int)HttpContext.Current.Session["UserId"];
                        var userd = db.PEdBdTables.FirstOrDefault(p => p.UserDbTableId == list9.Id);
                        var friendd = db.Friends.FirstOrDefault(p => p.IdFriend == list9.Id 
                                                                     && p.IdUser == userId);
                        if (friendd != null && userd!=null)
                        {
                            var oneOfList = mapper.Map<FriendsPageD>(userd);
                            oneOfList.BUserE = new UserE
                            {
                                Id = list9.Id,
                                Username = list9.UserName,
                                Email = list9.Email
                            };
                            friendsListD.ListOfFriends.Add(oneOfList);
                        }
                        else if (friendd != null)
                        {
                            var oneOfList = new FriendsPageD
                            {
                                BUserE = new UserE
                                {
                                    Id = list9.Id,
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

        protected bool setAddFriendD((int _userId, int? _friendId) valueTuple)
        {
            if (valueTuple._friendId == null) return false;
            using (var db = new UserContext())
            {
                try
                {
                    var verifyFriend = db.Friends.FirstOrDefault(v =>
                        v.IdFriend == valueTuple._friendId && v.IdUser == valueTuple._userId);
                    if (verifyFriend == null)
                    {
                        var friendTable = new FriendsDbTable()
                        {
                            IdUser = valueTuple._userId,
                            IdFriend = valueTuple._friendId,
                            User = db.UsersT.FirstOrDefault(v=>v.Id==valueTuple._userId),
                            Friend = db.UsersT.FirstOrDefault(g=>g.Id==valueTuple._friendId)
                        };
                        db.Friends.Add(friendTable);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    return false;
                }
            }
        }
        
        protected bool setDeleteFriendD((int _userId, int? _friendId) valueTuple)
        {
            if (valueTuple._friendId == null) return false;
            using (var db = new UserContext())
            {
                try
                {
                    var verifyFriend = db.Friends.FirstOrDefault(v =>
                        v.IdFriend == valueTuple._friendId && v.IdUser == valueTuple._userId);
                    if (verifyFriend != null)
                    {
                        db.Friends.Remove(verifyFriend);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    return false;
                }
            }
        }
    }
    
}
