using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Calendar
{
    public class EventDataModel
    {
        public string EventTitle { get; set; }
        public DateTime EventDay { get; set; }
    }
}