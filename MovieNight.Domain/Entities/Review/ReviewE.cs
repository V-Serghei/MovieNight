using System;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.Review
{
    public class ReviewE
    {
        public int? FilmId { get; set; }
        public string User { get; set; }
        public int? UserId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public TypeOfReview ReviewType { get; set; }
    }
}