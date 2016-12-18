using System.Drawing;
using System.Web;
using System.Web.Mvc;
using AntiBayanBot.Recognition;

namespace AntiBayanBot.Web.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image1, HttpPostedFileBase image2)
        {
            var detector = new ImageFeatureDeteсtor();

            var x = Request.Files;
            var bmp1 = new Bitmap((Bitmap)Image.FromStream(image1.InputStream, true, true), 200, 200);
            var bmp2 = new Bitmap((Bitmap)Image.FromStream(image2.InputStream, true, true), 200, 200);

            var d1 = detector.GetDescriptors(bmp1);
            var d2 = detector.GetDescriptors(bmp2);

            return Content(detector.IsSimilar(d1, d2).ToString());
        }

    }
}