﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;

namespace MovieNight.Domain.Entities.UserId
{
    public  class UserDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50,MinimumLength = 10)]
        public string Password { get; set; }

        [Required]
        [StringLength(30)]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime LastLoginDate { get; set; }

        
        public string LastIp { get; set; }

        [Required]
        public LevelOfAccess Role { get; set; }
        
        public bool Checkbox { get; set; }

        public string Salt { get; set; }
        
        public PEdBdTable PEdBdTable { get; set; }
        
        public ICollection<ViewListDbTable> ViewListEntries { get; set; }
        public ICollection<FriendsDbTable> FriendsDbTables { get; set; }
        
        public ICollection<BookmarkDbTable> Bookmark { get; set; }
        
        public virtual ICollection<UserAchievementDbTable> Achievements { get; set; }
        
    }
}
