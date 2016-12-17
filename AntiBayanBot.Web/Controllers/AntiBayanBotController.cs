using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;


namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        public readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            
            if (message.Type == MessageType.PhotoMessage)
            {
                // Download Photo
                var file = await Bot.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

                var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                //Get image features

                //Get existing features and compare

                //If bayan detected
                { 
                    await Bot.SendTextMessageAsync(message.Chat.Id, text: "-"+ GetPunishMessage(), replyToMessageId: message.MessageId);
                }
                //Else save feature in DB
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
            return messages[randomIndex];
        }
    }
}