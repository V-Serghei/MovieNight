using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.MailE
{
    public class MailDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int? SenderId { get; set; }
        [Required]
        public UserDbTable Sender { get; set; }
        [Required]
        public int? RecipientId { get; set; }
        [Required]
        public UserDbTable Recipient { get; set; }
        public string Theme { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public bool IsStarred { get; set; }
    }
}