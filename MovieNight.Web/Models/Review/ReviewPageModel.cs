using System.Collections.Generic;

namespace MovieNight.Web.Models.Review
{
    public class ReviewPageModel
    {
        public ReviewPageModel()
        {
            RGreat = new List<ReviewModel>();
            RFine = new List<ReviewModel>();
            RWaste = new List<ReviewModel>();
        }
        public int? FilmId { get; set; }
        public string FilmTitle { get; set; }
        public List<ReviewModel> RGreat { get; set; }
        public List<ReviewModel> RFine { get; set; }
        public List<ReviewModel> RWaste { get; set; }
    }
}