using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        private readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            bool bayanDetected = false;

            if (message.Type == MessageType.PhotoMessage)
            {
                // Download Photo
                var file = await Bot.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                string imageExt = file.FilePath.Split('.').Last();

                bayanDetected = new BayanDetector().DetectPhotoBayan(file.FileStream, file.FileSize, imageExt);
                
                if(!bayanDetected) //save feature in DB
                {
                    var obj = new
                    {
                        ChatId = message.Chat.Id,
                        UserId = message.From.Id,
                        PostDate = DateTime.UtcNow,
                        Features = "features"
                    };
                }
            }

            //Check text messages...?

            if (bayanDetected)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, text: GetPunishMessage(), replyToMessageId: message.MessageId);
            }            

            return Ok();
        }
        
        [NonAction]
        public string GetPunishMessage()
        {
            string[] messages =
            {
                "Где годнота?",
                "Заебал баяны постить",
                "Одни баяны пилишь",
                "Ну сколько можно баянить?",
                "БАЯН!",
                "Опять баян..."
            };

            var rnd = new Random(666);
            var randomIndex = rnd.Next(0, messages.Length);
            return "-" + messages[randomIndex];
        }
    }
}