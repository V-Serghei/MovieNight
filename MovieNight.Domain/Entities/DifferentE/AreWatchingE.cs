using System;
using System.Collections.Generic;

namespace MovieNight.Domain.Entities.DifferentE
{
    public class AreWatchingE
    {
        
        private List<string> _genre = new List<string>();
        public int? Id { get; set; }
        public string Title { get; set; }
        public string PosterImage { get; set; }
        public string Quote { get; set; }
        public string Description { get; set; }
        public DateTime ProductionYear { get; set; }
        public string Country { get; set; }

        public List<string> Genre
        {
            get => _genre;
            set => _genre = value;
        }

        public int CountWatching { get; set; }
        public float? Rating { get; set; }
        public bool Bookmark { get; set; }
        public bool BookmarkTomeOf { get; set; }
    }
}