using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities.UserId
{
    public class UserVerification
    {
        public bool IsVerified { get; set; } = false;
        public LogInData LogInData { get; set; }
        public int UserId { get; set; }
    }

    public class UserRegister
    {
        public bool SuccessUniq { get; set; } = false;
        public string StatusMsg { get; set; }
        public int UserId { get; set; }
    }
}
