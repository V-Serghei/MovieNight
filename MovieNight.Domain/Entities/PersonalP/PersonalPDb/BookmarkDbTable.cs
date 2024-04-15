using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class BookmarkDbTable
    {//We should think about it
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DateTime TimeExpire { get; set; }
        
        
    }
}