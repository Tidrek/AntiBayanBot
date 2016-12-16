using System.Web.Mvc;

namespace AntiBayanBot.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Content("OK");
        }
    }
}