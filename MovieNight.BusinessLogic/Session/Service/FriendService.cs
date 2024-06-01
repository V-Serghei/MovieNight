using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class FriendService : FriendAPI, IFriendsService
    {
        public FriendsPageD getFriendDate(int? id)
        {
            return getFriendDateDB(id);
        }

        public FriendsListD getListOfUsers( int _skipParameter, string _searchParameter)
        {
            return getListOfUsersD(_skipParameter, _searchParameter);
        }
        public FriendsListD getListOfFriends(int _skipParameter, string _searchParameter)
        {
            return getListOfFriendsD(_skipParameter, _searchParameter);
        }

        public bool setAddFriend((int _userId, int? _friendId) valueTuple)
        {
            return setAddFriendD(valueTuple);
        }
        
        public bool setDeleteFriend((int _userId, int? _friendId) valueTuple)
        {
            return setDeleteFriendD(valueTuple);
        }
        public int GetTotalUserCount(string _searchParameter)
        {
            return GetTotalUserCountD(_searchParameter);
        }
        public int GetTotalFriendsCount(string _searchParameter)
        {
            return GetTotalFriendsCountD(_searchParameter);
        }

        #region Information - friends movies

        /// <summary>
        /// friend ratings for a certain movie
        /// </summary>
        /// <param name="idU"></param>
        /// <returns>List<MovieTemplateInfE />
        /// </returns>
        public List<ScoresFriendsGaveTheMovieE> GetFriendsMovie(int? idU)
        {
            return GetFriendsMovieD(idU);
        }

        public List<ScoresFriendsGaveTheMovieE> GetFriendsMovieAll(int? movieId)
        {
            return GetFriendsMovieAllD(movieId);
        }

        public int GetCountFriendsGrade(int? idU)
        {
            return GetCountFriendsGradeD(idU);
        }

        #endregion
        
    }
}
