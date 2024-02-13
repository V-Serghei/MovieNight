using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

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
        }
    }
}