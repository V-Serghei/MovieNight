using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
{
    public sealed class MovieCardDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(500,MinimumLength = 1)]
        public string Title { get; set; }
        
        [Required]
        public string ImageUrl { get; set; }
        
        public string Description { get; set; }

        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public MovieDbTable Movie { get; set; }
    }
}