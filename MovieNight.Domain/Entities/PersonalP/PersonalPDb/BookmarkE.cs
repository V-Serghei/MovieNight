using System;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class BookmarkE
    {
        public string Msg { get; set; } 
        public bool Success { get; set; }
        public int IdUser { get; set; }
        public int IdMovie { get; set; }
        
        public DateTime TimeAdd { get; set; }

        public bool BookmarkTimeOf { get; set; } = false;
        public bool BookMark { get; set; } = false;


    }
}