using System.Web.Optimization;

public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.IgnoreList.Clear();

        // ===== CORE CSS (на всех страницах) =====
        var coreCss = new StyleBundle("~/content/core");
        coreCss.Transforms.Clear();
        coreCss.Transforms.Add(new NoMinifyCssTransform());
        coreCss
            .Include("~/wwwroot/lib/bootstrap/css/bootstrap.min.css", new CssRewriteUrlTransform())
            .Include("~/wwwroot/lib/metisMenu/metisMenu.min.css", new CssRewriteUrlTransform())
            .Include("~/wwwroot/lib/node-waves/waves.min.css", new CssRewriteUrlTransform())
            .Include("~/wwwroot/lib/remixicon/remixicon.css", new CssRewriteUrlTransform())
            .Include("~/Client/Content/icons.min.css", new CssRewriteUrlTransform())
            .Include("~/Client/Content/app.min.css", new CssRewriteUrlTransform())
            .Include("~/Client/Content/hbar.css", new CssRewriteUrlTransform())
            .Include("~/Client/Content/override.css", new CssRewriteUrlTransform());
            
        bundles.Add(coreCss);

    }
}