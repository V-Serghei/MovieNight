﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities
{
    public class UserE
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
    }
}