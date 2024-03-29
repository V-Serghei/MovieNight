using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public DateTime LoginTime { get; set; }
        public bool RememberMe { get; set; } = false;

    }
}