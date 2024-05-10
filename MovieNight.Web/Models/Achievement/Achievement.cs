namespace MovieNight.Web.Models.Achievement
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Unlocked { get; set; }
        public string UserId { get; set; }
    }
}