    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
    {
        public sealed class CastMemDbTable
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            
            [Required]
            public string Name { get; set; }
            
            [Required]
            public string Role { get; set; }
            
            [Required]
            public string ImageUrl { get; set; }

            // Link "many-to-many" with the table MovieDbTable
            public ICollection<MovieDbTable> Movies { get; set; }
        }
    }