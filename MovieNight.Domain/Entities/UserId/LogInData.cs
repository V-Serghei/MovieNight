using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.UserId
{
    public class LogInData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Ip { get; set; }
        public DateTime LoginTime { get; set; }
        public bool RememberMe { get; set; } = false;
        
        public LevelOfAccess Role { get; set; }
        
        public string Agent { get; set; }

        public string Avatar { get; set; }


    }
}
