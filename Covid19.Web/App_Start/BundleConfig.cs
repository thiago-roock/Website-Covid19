using System.Web;
using System.Web.Optimization;

namespace Covid19.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/Covid.css"));
        }
    }
}
