using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.AchievementE
{
    public class AchievementDbTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public AchievementType AchievementType { get; set; }
        public string ImgA { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string AdditionalRecords { get; set; }
        public int SuccessСount { get; set; }
        public bool Unlocked { get; set; }
    }
    
}