using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.Web.Attributes;

namespace MovieNight.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : MasterController
    {
        // GET: Admin

        [HttpPost]
        [AdminMod]
        public ActionResult UploadingTheFileOfAddingMoviesDb(HttpPostedFileBase file)
        {
            if (file != null)
            {
                
                var filePath = Path.Combine(Server.MapPath("~/uploads"), file.FileName);
                file.SaveAs(filePath);

            }

            return View("Index");
        }

        [HttpGet]
        [AdminMod]
        public ActionResult Index()
        {

            return View();
        }
    }
}