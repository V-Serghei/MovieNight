using System;

namespace MovieNight.Web.Models.Calendar
{
    public class CalendarEvent
    {

        public string Title { get; set; }

        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        public string Category { get; set; }
    }
}