using System.ComponentModel.DataAnnotations;

namespace MovieNight.Web.Models.PersonalP.RecoverM
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Code { get; set; }
    
        [Required]
        public string Email { get; set; }
    }
}