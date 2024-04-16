using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Domain.enams;

namespace MovieNight.Web.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Ip { get; set; }
        
        public DateTime LoginTime { get; set; }
        
        public LevelOfAccess Role { get; set; }
        public bool RememberMe { get; set; }
        
        public string Agent { get; set; }

        public string Avatar { get; set; }

    }
}