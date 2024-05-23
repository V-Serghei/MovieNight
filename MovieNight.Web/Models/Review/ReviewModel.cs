using System;
using MovieNight.Domain.enams;

namespace MovieNight.Web.Models.Review
{
    public class ReviewModel
    {
        public int Id { get; set; }
        public int? FilmId { get; set; }
        public string User { get; set; }
        public int? UserId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public TypeOfReview ReviewType { get; set; }
    }
}