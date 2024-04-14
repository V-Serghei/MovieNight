using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieNight.Domain.Entities.MovieM.EfDbEntities
{
    public class MovieDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(500,MinimumLength = 1)]
        public string Title { get; set; }
        
        
        public string PosterImage { get; set; }
        
        [StringLength(500,MinimumLength = 1)]
        public string Quote { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime ProductionYear { get; set; }
        
        [Required]
        public string Country { get; set; }
        
        // Property for storing genres in JSON format
        public string Genres { get; set; }

        [Required]
        [StringLength(500,MinimumLength = 3)]
        public string Location { get; set; }
        
        [Required]
        public string Director { get; set; }
        
        //[DataType(DataType.Time)]
        public DateTime Duration { get; set; }
        
        [Required]
        [Range(0,10)]
        public float MovieNightGrade { get; set; }
        
        [Required]
        [StringLength(30,MinimumLength = 1)]
        public string Certificate { get; set; }
        
        public string ProductionCompany { get; set; }
        
        public string Budget { get; set; }
        
        public string GrossWorldwide { get; set; }
        
        public string Language { get; set; }
        
        // Link "one-to-many" with CastMemberDbTable
        public virtual ICollection<CastMemDbTable> CastMembers { get; set; }
        
        // Link "one-to-many" with MovieCardDbTable table
        public virtual ICollection<MovieCardDbTable> MovieCards { get; set; }
        
        // Link "one-to-many" with InterestingFactDbTable table
        public virtual ICollection<InterestingFactDbTable> InterestingFacts { get; set; }
    }
}