using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using File = System.IO.File;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Drawing;
using System.Security.Cryptography;


namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        public readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            bool bayanDetected = false;

            if (message.Type == MessageType.PhotoMessage)
            {
                // Download Photo
                var file = await Bot.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                string imageExt = file.FilePath.Split('.').Last().ToLower();

                if (imageExt == "gif")
                {
                    //Compute md5
                    string hash = null;
                    using (var md5 = MD5.Create())
                    {
                        byte[] buffer = null;
                        file.FileStream.Read(buffer, 0, file.FileSize);
                        hash = BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "‌​").ToLower(); //standard looking md5              
                    }
                    //Get existing hashes and compare
                    //Set bayanDetected = true if hash is found 
                }
                else { 
                    //Get image features
                    var bitmap = new Bitmap(file.FileStream);

                    //Get existing features and compare
                    //Set bayanDetected = true if chance is high            
                }

                if(bayanDetected)
                { 
                    await Bot.SendTextMessageAsync(message.Chat.Id, text: GetPunishMessage(), replyToMessageId: message.MessageId);
                }
                else
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
            return "-" + messages[randomIndex];
        }
    }
}