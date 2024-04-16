using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.UserId
{
    public class RegData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime RegDateTime { get; set; }
        public string Ip { get; set; }
        public bool Checkbox { get; set; } = false;
        
        public LevelOfAccess Role { get; set; }
        
    }
}
