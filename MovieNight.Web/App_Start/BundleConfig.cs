using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;


namespace MovieNight.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/bootstrap/css").Include(
                "~/Content/bootstrap.min.css", new CssRewriteUrlTransform()).Include(
                "~/Content/icons.min.css", new CssRewriteUrlTransform()).Include(
                "~/Content/app.min.css", new CssRewriteUrlTransform()));
            // bundles.Add(new StyleBundle("~/bundles/icons/css").Include(
            //     "~/Content/icons.min.css", new CssRewriteUrlTransform()));
            // bundles.Add(new StyleBundle("~/bundles/app/css").Include(
            //     "~/Content/app.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/vendor/js").Include(
                "~/scripts/vendor.min.js"));
             bundles.Add(new ScriptBundle("~/bundles/app/js").Include(
                 "~/scripts/app.min.js"));

             bundles.Add(new ScriptBundle("~/bundles/AdditionalScripts/js").Include(
                 "~/vendor/AdditionalScripts.js"));

             bundles.Add(new StyleBundle("~/bundles/slider/css").Include(
                 "~/vendor/myStyle.css", new CssRewriteUrlTransform()));

            //page of friends
            
             bundles.Add(new StyleBundle("~/bundles/custombox/css").Include(
                 "~/vendor/custombox.min.css", new CssRewriteUrlTransform()));
             bundles.Add(new ScriptBundle("~/bundles/custombox/js").Include(
                "~/vendor/custombox.min.js"));

            //viewed list
            
             bundles.Add(new StyleBundle("~/bundles/dataTables_bootstrap4/css").Include(
                 "~/vendor/datatables/dataTables.bootstrap4.css", new CssRewriteUrlTransform()).Include(
                 "~/vendor/datatables/responsive.bootstrap4.css", new CssRewriteUrlTransform()).Include(
                 "~/vendor/datatables/buttons.bootstrap4.css", new CssRewriteUrlTransform()).Include(
                 "~/vendor/datatables/select.bootstrap4.css", new CssRewriteUrlTransform()));
             // bundles.Add(new StyleBundle("~/bundles/dataTables_responsive_bootstrap4/css").Include(
             //     "~/vendor/datatables/responsive.bootstrap4.css", new CssRewriteUrlTransform()));
             // bundles.Add(new StyleBundle("~/bundles/dataTables_buttons_bootstrap4/css").Include(
             //     "~/vendor/datatables/buttons.bootstrap4.css", new CssRewriteUrlTransform()));
             // bundles.Add(new StyleBundle("~/bundles/dataTables_select_bootstrap4/css").Include(
             //         "~/vendor/datatables/select.bootstrap4.css", new CssRewriteUrlTransform()));

             bundles.Add(new ScriptBundle("~/bundles/jquery_dataTables_min/js").Include(
                   "~/vendor/datatables/jquery.dataTables.min.js"));
             bundles.Add(new ScriptBundle("~/bundles/dataTables_bootstrap4/js").Include(
                          "~/vendor/datatables/dataTables.bootstrap4.js"));
            bundles.Add(new ScriptBundle("~/bundles/dataTables_responsive/js").Include(
                     "~/vendor/datatables/dataTables.responsive.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/dataTables_bootstrap4_min/js").Include(
                     "~/vendor/datatables/responsive.bootstrap4.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/dataTables_buttons_min/js").Include(
                     "~/vendor/datatables/dataTables.buttons.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/buttons_bootstrap4_min/js").Include(
                     "~/vendor/datatables/buttons.bootstrap4.min.js").Include(
                "~/vendor/datatables/buttons.html5.min.js").Include(
                "~/vendor/datatables/buttons.flash.min.js").Include(
                "~/vendor/datatables/buttons.print.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/buttons_html5_min/js").Include(
            //          "~/vendor/datatables/buttons.html5.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/buttons_flash_min/js").Include(
            //          "~/vendor/datatables/buttons.flash.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/buttons_print_min/js").Include(
            //          "~/vendor/datatables/buttons.print.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/dataTables_keyTable_min/js").Include(
                     "~/vendor/datatables/dataTables.keyTable.min.js").Include(
                "~/vendor/datatables/dataTables.select.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/dataTables_select_min/js").Include(
            //          "~/vendor/datatables/dataTables.select.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/pdfmake_min/js").Include(
                     "~/vendor/pdfmake/pdfmake.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/vfs_fonts/js").Include(
                     "~/vendor/pdfmake/vfs_fonts.js"));
            bundles.Add(new ScriptBundle("~/bundles/datatables_init/js").Include(
                     "~/scripts/pages/datatables.init.js"));
            //statistic
            bundles.Add(new StyleBundle("~/bundles/jquery_jvectormap_1_2_2/css").Include(
                    "~/vendor/jquery-vectormap/jquery-jvectormap-1.2.2.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/apexcharts_min/js").Include(
                "~/vendor/apexcharts/apexcharts.min.js").Include(
                "~/vendor/jquery-sparkline/jquery.sparkline.min.js").Include(
                "~/vendor/jquery-vectormap/jquery-jvectormap-1.2.2.min.js").Include(
                "~/vendor/jquery-vectormap/jquery-jvectormap-world-mill-en.js").Include(
                "~/vendor/peity/jquery.peity.min.js").Include(
                "~/scripts/pages/dashboard-2.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/jquery_sparkline_min/js").Include(
            //     "~/vendor/jquery-sparkline/jquery.sparkline.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/jquery_jvectormap_1_2_2/js").Include(
            //     "~/vendor/jquery-vectormap/jquery-jvectormap-1.2.2.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/jquery_jvectormap_world_mill_en/js").Include(
            //     "~/vendor/jquery-vectormap/jquery-jvectormap-world-mill-en.js"));
            // bundles.Add(new ScriptBundle("~/bundles/peity_min/js").Include(
            //     "~/vendor/peity/jquery.peity.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/dashboard_init/js").Include(
            //         "~/scripts/pages/dashboard-2.init.js"));

            //News

            bundles.Add(new StyleBundle("~/bundles/magnific_popup/css").Include(
                    "~/vendor/magnific-popup/magnific-popup.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/jquery.magnific_popup_min/js").Include(
                    "~/vendor/magnific-popup/jquery.magnific-popup.min.js").Include(
                "~/vendor/pages/gallery.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/gallery_init/js").Include(
            //         "~/vendor/pages/gallery.init.js"));

            //Are watching

            bundles.Add(new StyleBundle("~/bundles/tablesaw/css").Include(
                    "~/vendor/tablesaw/tablesaw.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/tablesaw/js").Include(
                    "~/vendor/tablesaw/tablesaw.js").Include(
                "~/scripts/pages/tablesaw.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/tablesaw_init/js").Include(
            //         "~/scripts/pages/tablesaw.init.js"));
            
            //calendar
            bundles.Add(new StyleBundle("~/bundles/fullcalendar/css").Include(
                "~/vendor/fullcalendar/fullcalendar.min.css", new CssRewriteUrlTransform()));
             
               
            bundles.Add(new ScriptBundle("~/bundles/moment/js").Include(
                "~/vendor/moment/moment.min.js").Include(
                "~/vendor/jquery-ui/jquery-ui.min.js").Include(
                "~/vendor/fullcalendar/fullcalendar.min.js").Include(
                "~/scripts/pages/calendar.init.js").Include(
                "~/vendor/bootstrap-datepicker/bootstrap-datepicker.min.js").Include(
                "~/scripts/pages/form-pickers.init.js").Include(
                "~/vendor/clockpicker/bootstrap-clockpicker.min.js").Include(
                "~/vendor/bootstrap-daterangepicker/daterangepicker.js"));
            // bundles.Add(new ScriptBundle("~/bundles/jquery-ui/js").Include(
            //     "~/vendor/jquery-ui/jquery-ui.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/fullcalendar/js").Include(
            //     "~/vendor/fullcalendar/fullcalendar.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/calendar_init/js").Include(
            //     "~/scripts/pages/calendar.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker/js").Include(
            //     "~/vendor/bootstrap-datepicker/bootstrap-datepicker.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/form-pickers_init/js").Include(
            //     "~/scripts/pages/form-pickers.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/clockpicker/js").Include(
            //     "~/vendor/clockpicker/bootstrap-clockpicker.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/bootstrap-daterangepicker/js").Include(
            //     "~/vendor/bootstrap-daterangepicker/daterangepicker.js"));

            //email
            bundles.Add(new ScriptBundle("~/bundles/inbox/js").Include(
               "~/scripts/pages/inbox.js").Include(
                "~/vendor/summernote/summernote-bs4.min.js"));
            // bundles.Add(new ScriptBundle("~/bundles/summernote_bs4/js").Include(
            //    "~/vendor/summernote/summernote-bs4.min.js"));
            bundles.Add(new StyleBundle("~/bundles/summernote_bs4/css").Include(
               "~/vendor/summernote/summernote-bs4.css", new CssRewriteUrlTransform()));

            //file
            bundles.Add(new StyleBundle("~/bundles/dropzone/css").Include(
                "~/vendor/dropzone/dropzone.min.css", new CssRewriteUrlTransform()));
            
            bundles.Add(new ScriptBundle("~/bundles/dropzone/js").Include(
                    "~/vendor/dropzone/dropzone.min.js").Include(
                "~/scripts/pages/form-fileuploads.init.js").Include(
                "~/scripts/pages/form-advanced.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/fileuploads/js").Include(
            //         "~/scripts/pages/form-fileuploads.init.js"));
            // bundles.Add(new ScriptBundle("~/bundles/form-advanced/js").Include(
            //     "~/scripts/pages/form-advanced.init.js"));


                //Review page
            bundles.Add(new ScriptBundle("~/bundles/kanban/js").Include(
                        "~/scripts/pages/kanban.init.js"));
                


        }
    }
}