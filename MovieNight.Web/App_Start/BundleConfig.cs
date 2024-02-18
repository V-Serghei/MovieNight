using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI.WebControls;

namespace MovieNight.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundle)
        {
            bundle.Add(new StyleBundle("~/bundles/bootstrap/css").Include(
                "~/Content/bootstrap.min.css", new CssRewriteUrlTransform()));
            bundle.Add(new StyleBundle("~/bundles/icons/css").Include(
                "~/Content/icons.min.css", new CssRewriteUrlTransform()));
            bundle.Add(new StyleBundle("~/bundles/app/css").Include(
                "~/Content/app.min.css", new CssRewriteUrlTransform()));
            bundle.Add(new ScriptBundle("~/bundles/vendor/js").Include(
                "~/scripts/vendor.min.js"));
            bundle.Add(new ScriptBundle("~/bundles/app/js").Include(
                "~/scripts/app.min.js"));

            bundle.Add(new StyleBundle("~/bundles/slider/css").Include(
                "~/vendor/myStyle.css", new CssRewriteUrlTransform()));
            
            //page of friends
            bundle.Add(new StyleBundle("~/bundles/custombox/css").Include(
                "~/vendor/custombox.min.css", new CssRewriteUrlTransform()));
            bundle.Add(new ScriptBundle("~/bundles/custombox/js").Include(
               "~/vendor/custombox.min.js"));

            //calendar
            bundle.Add(new StyleBundle("~/bundles/fullcalendar/css").Include(
                "~/vendor/fullcalendar/fullcalendar.min.css", new CssRewriteUrlTransform()));
            bundle.Add(new ScriptBundle("~/bundles/moment/js").Include(
               "~/vendor/moment/moment.min.js"));
            bundle.Add(new ScriptBundle("~/bundles/jquery-ui/js").Include(
                "~/vendor/jquery-ui/jquery-ui.min.js"));
            bundle.Add(new ScriptBundle("~/bundles/fullcalendar/js").Include(
                "~/vendor/fullcalendar/fullcalendar.min.js"));
            bundle.Add(new ScriptBundle("~/bundles/calendar_init/js").Include(
                "~/scripts/pages/calendar.init.js"));

        }
    }
}