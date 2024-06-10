using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.Domain.Entities.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IFriendsService 
    {
        FriendsPageD getFriendDate(int? id);
        FriendsListD getListOfUsers(int _skipParameter, string _searchParameter);
        FriendsListD getListOfFriends(int _skipParameter, string _searchParameter);
        bool setAddFriend((int _userId, int? _friendId) valueTuple);
        bool setDeleteFriend((int _userId, int? _friendId) valueTuple);
        int GetTotalUserCount(string _searchParameter);
        int GetTotalFriendsCount(string _searchParameter);
        #region Information - friends movies

        /// <summary>
        /// friend ratings for a certain movie
        /// </summary>
        
        List<ScoresFriendsGaveTheMovieE> GetFriendsMovie(int? idU);
        #endregion

        List<ScoresFriendsGaveTheMovieE> GetFriendsMovieAll(int? movieId);
        int GetCountFriendsGrade(int? idU);
    }
}
