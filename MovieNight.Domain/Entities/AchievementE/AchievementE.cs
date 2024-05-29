using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.AchievementE
{
    public class AchievementE
    {
       
        public int Id { get; set; }
        public string Name { get; set; }
        public AchievementType AchievementType { get; set; }
        public string ImgA { get; set; }

        public string Description { get; set; }
        public string Condition { get; set; }
        public string AdditionalRecords { get; set; }
        public int SuccessСount { get; set; }
        public int ProgressСount { get; set; }

        public bool Unlocked { get; set; }
    }
}