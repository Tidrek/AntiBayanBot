using System;
using System.Drawing;
using System.Web;
using System.Web.Mvc;

namespace AntiBayanBot.Web.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            var img = Image.FromStream(image.InputStream);
            var messageData = new Core.Models.MessageData
            {
                MessageId = 0,
                ChatId = 0,
                UserId = 0,
                DateTimeAdded = DateTime.UtcNow,
                UserFullName = "Test",
                UserName = "Test"
            };
            var result = Recognition.BayanDetector.DetectPhotoBayan(img, messageData);

            return Content(
                result.IsBayan ?
                $"Баян. Уровень баянства: {result.Bayanity}." :
                "Не баян.");
        }
    }
}