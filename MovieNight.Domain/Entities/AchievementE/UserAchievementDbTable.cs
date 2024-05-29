using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.AchievementE
{
    public class UserAchievementDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserDbTable User { get; set; }
        public int AchievementId { get; set; }
        public virtual AchievementDbTable Achievement { get; set; }
    }
}