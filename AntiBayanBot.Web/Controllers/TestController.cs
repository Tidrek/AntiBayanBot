using System;
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

            //var bmp1 = (Bitmap)Image.FromStream(image1.InputStream);
            //var bmp2 = (Bitmap)Image.FromStream(image2.InputStream);

            //var d1 = detector.GetDescriptors(bmp1);
            //var d2 = detector.GetDescriptors(bmp2);

            //return Content(detector.GetSimilarity(d1, d2).ToString(CultureInfo.InvariantCulture));

            var image = Image.FromStream(image1.InputStream);
            var messageData = new Core.Models.MessageData
            {
                MessageId = 0,
                ChatId = 0,
                UserId = 0,
                DateTimeAdded = DateTime.UtcNow,                
                UserFullName = "foo bar",
                UserName = "foobar"
            };
            var result = Recognition.BayanDetector.DetectPhotoBayan(image, messageData);

            return Content(result.IsBayan ? $"BAYAN. Bayanity is: {result.Bayanity}" : "OK");
        }

    }
}