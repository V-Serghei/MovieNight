using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Calendar
{
    public class EventDataModel
    {
        public string title { get; set; }
        public string category { get; set; }
        public string beginning { get; set; }
        public string ending { get; set; }

    }
}