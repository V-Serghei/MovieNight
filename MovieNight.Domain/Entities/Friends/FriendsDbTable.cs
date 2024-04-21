using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.Friends
{
    public class FriendsDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int? IdUser { get; set; }
        [Required]
        public UserDbTable User { get; set; }
        [Required]
        public int? IdFriend { get; set; }
        [Required]
        public UserDbTable Friend { get; set; }
    }
}