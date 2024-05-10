using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Mail
{
    public class InboxModel
    {
        public bool IsChecked { get; set; }
        public string SenderName { get; set; }
        
        public string RecipientName { get; set; }
        public string Theme { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        
        public bool IsStarred { get; set; }
    }
}