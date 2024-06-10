using System.ComponentModel.DataAnnotations;

namespace MovieNight.Web.Models.PersonalP.RecoverM
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}