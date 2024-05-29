using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class BookmarkDbTable
    {//We should think about it
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MovieId { get; set; }
        
        public int UserId { get; set; }
        
        public virtual UserDbTable User { get; set; } 
        
        public virtual MovieDbTable Movie { get; set; }
        
        public DateTime TimeAdd { get; set; }
        
        public bool BookmarkTimeOf { get; set; } = false;

        public bool BookMark { get; set; } = false;


    }
}