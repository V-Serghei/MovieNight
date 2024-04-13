using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
{
    public class InterestingFactDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100,MinimumLength = 10)]
        public string FactName { get; set; }
        
        [Required]
        [StringLength(500,MinimumLength = 20)]
        public string FactText { get; set; }

        // Внешний ключ на таблицу MovieDbTable
        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public virtual MovieDbTable Movie { get; set; }
    }
}