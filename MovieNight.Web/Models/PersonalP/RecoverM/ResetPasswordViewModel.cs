using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;

namespace MovieNight.Web.Models.PersonalP.RecoverM
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Code { get; set; }
    
        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(10)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}