using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.Review
{
    public class ReviewDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int FilmId { get; set; }
        [Required]
        public MovieDbTable Film { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public UserDbTable User { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public TypeOfReview ReviewType { get; set; }
    }
}