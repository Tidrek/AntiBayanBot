using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Drawing;

namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        private readonly TelegramBotClient _bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            var result = new Core.Models.BayanResult();
            try
            {
                if (message?.Type == MessageType.PhotoMessage)
                {
                    // Download Photo
                    var file = await _bot.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                    var imageExt = file.FilePath.Split('.').Last();

                    //bayanDetected = new BayanDetector().DetectPhotoBayan(file.FileStream, file.FileSize, imageExt);
                    var bitmap = new Bitmap(file.FileStream);
                    var messageData = new Core.Models.MessageData
                    {
                        MessageId = message.MessageId,
                        ChatId = message.Chat.Id,
                        UserId = message.From.Id,
                        DateTimeAdded = DateTime.UtcNow,
                        UserFullName = message.From.FirstName + " " + message.From.LastName,
                        UserName = message.From.Username
                    };
                    result = Recognition.BayanDetector.DetectPhotoBayan(bitmap, messageData);

                }

                //Check text messages...?

                if (result.IsBayan)
                {
                    try
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: GetPunishMessage(), replyToMessageId: message.MessageId);                        
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) //Message was deleted before resolve could happen
                    {
                        if (ex.Message == "Bad Request: message not found") { return Ok(); }
                    }
                    try
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: "Пруф", replyToMessageId: result.OriginalImage.MessageId);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) //Message was deleted before resolve could happen
                    {
                        if (ex.Message == "Bad Request: message not found") {
                            
                            await _bot.SendTextMessageAsync(message.Chat.Id, text: GetProofForDeletedMessage(
                                result.OriginalImage.UserName,
                                result.OriginalImage.UserFullName,
                                result.OriginalImage.DateTimeAdded));
                        }
                    }                    
                }

            }
            catch (Exception ex)
            {   //LOG NAHUY
                await _bot.SendTextMessageAsync(message.Chat.Id, text: "```" + ex.Message + "```", parseMode:ParseMode.Markdown );
                await _bot.SendTextMessageAsync(message.Chat.Id, text: "```" + ex.StackTrace + "```", parseMode: ParseMode.Markdown);
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

            var rnd = new Random();
            var randomIndex = rnd.Next(0, messages.Length);
            return messages[randomIndex];
        }

        [NonAction]
        public string GetProofForDeletedMessage(string userName, string userFullName, DateTime dateTimeAdded)
        {
            var result = string.IsNullOrEmpty(userName)? $"@{userFullName}" : $"@{userName}";
            result += string.Format(" запостил {0:yyyy-MM-dd} в {0: HH:mm}, но за каким-то хером удалил", dateTimeAdded);
            return result;
        }
    }
}