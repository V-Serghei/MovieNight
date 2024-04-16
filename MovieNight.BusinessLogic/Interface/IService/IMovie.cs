using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IMovie
    {
        MovieTemplateInfE GetMovieInf(int? id);

        Task<BookmarkE> SetNewBookmark((int,int) movieid);
        bool GetInfBookmark((int,int) movieid);

        List<ListOfFilmsE> GetListPlain(int? userId);
    }
}