using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities.MailE
{
    public class InboxD
    {
        public int Id { get; set; }
        public bool IsChecked { get; set; }
        public string SenderName { get; set; }
        public int? SenderId { get; set; }
        public string RecipientName { get; set; }
        public int? RecipientId { get; set; }
        public string Theme { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        
        public bool IsStarred { get; set; }
    }
}
