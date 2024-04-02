using MovieNight.Domain.enams;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities.Notification
{
    public class EventE
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public EventColor Category { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
    }
}
