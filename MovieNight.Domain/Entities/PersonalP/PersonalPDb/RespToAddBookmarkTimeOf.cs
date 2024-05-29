using System.Collections.Generic;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class RespToAddBookmarkTimeOf
    {
        public bool IsSuccese { get; set; }
        public BookmarkE Bookmark { get; set; }
        public MovieTemplateInfE MovieInTimeOfBookmark { get; set; }
        public string RespMsg { get; set; }
    }
}