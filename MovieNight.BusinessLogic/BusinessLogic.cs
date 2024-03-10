using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Interface;
using MovieNight.BusinessLogic.Session;

namespace MovieNight.BusinessLogic
{
    public class BusinessLogic
    {
        public ISession Session()
        {
            return new SessionLog();
        }
        
    }
}
