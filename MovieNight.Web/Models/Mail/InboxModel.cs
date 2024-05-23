using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Mail
{
    public class InboxModel
    {
        public int Id { get; set; }
        public bool IsChecked { get; set; }
        
        public string RecipientName { get; set; }
        public DateTime Date { get; set; }
        public bool IsStarred { get; set; }
    }
}