using System.Web.Mvc;
using System.Web.Routing;

namespace AntiBayanBot.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Default", "{controller}/{action}", new { controller = "Home", action = "Index" });
        }
    }
}