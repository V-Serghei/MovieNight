using System;

namespace MovieNight.Web.Models.PersonalP.Bookmark
{
    public class BookmarkModel
    {
        public string Msg { get; set; } = "Bookmark";
        
        public bool Success { get; set; }
        public int IdUser { get; set; }
        
        public int IdMovie { get; set; }

        public bool BookmarkTimeOf { get; set; } = false;
        
        public bool BookMark { get; set; } = false;
        
        public DateTime TimeAdd { get; set; }



    }
}