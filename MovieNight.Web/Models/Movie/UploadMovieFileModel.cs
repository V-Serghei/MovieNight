using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Movie
{
    public class UploadMovieFileModel
    {
        public HttpPostedFileBase file { get; set; }

        [Required]
        [RegularExpression(@"^text/plain$", ErrorMessage = "Only TXT files are allowed.")]
        public string FileType { get; set; }
    }

}