﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
{
    public class MovieCardDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50,MinimumLength = 3)]
        public string Title { get; set; }
        
        [Required]
        public string ImageUrl { get; set; }
        
        public string Description { get; set; }

        // Внешний ключ на таблицу MovieDbTable
        [ForeignKey("MovieDbTable")]
        public int MovieId { get; set; }
        public virtual MovieDbTable Movie { get; set; }
    }
}