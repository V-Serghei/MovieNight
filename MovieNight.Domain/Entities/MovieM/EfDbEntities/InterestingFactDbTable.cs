    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
    {
        public sealed class InterestingFactDbTable
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            
            [Required]
            [StringLength(200,MinimumLength = 1)]
            public string FactName { get; set; }
            
            [Required]
            [StringLength(1500,MinimumLength = 1)]
            public string FactText { get; set; }

            // External key to MovieDbTable table
            [ForeignKey("Movie")]
            public int MovieId { get; set; }
            public MovieDbTable Movie { get; set; }
        }
    }