using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class ViewListDbTable
    {//Resolve the conflict table
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int UserValues { get; set; }
        
        public DateTime ReviewDate { get; set; }
        
        public string UserComment { get; set; }
        
        public int UserViewCount { get; set; }
        
        public TimeSpan TimeSpent { get; set; }
        
        public int UserId { get; set; } 
        public int MovieId { get; set; } 
    
        [ForeignKey("UserId")]
        public UserDbTable User { get; set; }

        [ForeignKey("MovieId")]
        public MovieDbTable Movie { get; set; } 
    
        
    }
}