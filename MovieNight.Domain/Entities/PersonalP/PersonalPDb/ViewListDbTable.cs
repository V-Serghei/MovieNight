using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class ViewListDbTable
    {
        
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public int MovieId { get; set; } 
        
        public virtual UserDbTable User { get; set; } 
        public virtual MovieDbTable Movie { get; set; }
        
        public int UserValues { get; set; }           
                                               
        public DateTime ReviewDate { get; set; }      
                                               
        public string UserComment { get; set; }       
                                               
        public int UserViewCount { get; set; }        
                                               
        public DateTime TimeSpent { get; set; }       
        
        // //Resolve the conflict table
        // // Navigation property to represent the associated movie for this view list entry
        // public int MovieId { get; set; }
        // public MovieDbTable Movie { get; set; }
        //
        // // Foreign key property to establish the relationship with UserDbTable
        // public int UserId { get; set; }
        // public UserDbTable User { get; set; }
        
    }
}