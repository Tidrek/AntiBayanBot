using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using AntiBayanBot.Core;

namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        private readonly TelegramBotClient _bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            
            Logger.Info($"New message.{Environment.NewLine}" +
                        $"Chat ID: {message.Chat.Id}, chat title: {message.Chat.Title},{Environment.NewLine}" +
                        $"User ID: {message.From.Id}, username: {message.From.Username}, user full name: {GetUserFullName(message.From.FirstName, message.From.LastName)}." + Environment.NewLine +
                        $"Message: {message.Text}");

            var result = new Core.Models.BayanResult();
            //For a case when single/multiple url-images w/ or w/o message text are in a single message. Contains only bayans
            var bayanResults = new List<Core.Models.BayanResult>();
            try
            {
                if (message?.Type == MessageType.PhotoMessage)
                {
                    //---------------------CHECK FORWARD?

                    // Download Photo
                    var file = await _bot.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                    //var imageExt = file.FilePath.Split('.').Last();
                    var bitmap = new Bitmap(file.FileStream);
                    result = GetBayanResult(bitmap, message);

                }
                else if (message?.Type == MessageType.TextMessage)
                {   //------------------------BOT COMMANDS------------------------
                    if (message.Text[0] == '/')
                    {
                        switch (message.Text)
                        {
                            case "/stats@AntiBayanBot":
                            case "/stats":
                                var statMessage = GetStatsMessage(message.Chat.Id, message.Chat.Title);
                                await _bot.SendTextMessageAsync(message.Chat.Id, text: statMessage, parseMode: ParseMode.Html);
                                break;
                        }
                    }
                    //find links to images
                    if (message.Entities != null)
                    {
                        foreach (var msgEntity in message.Entities)
                        {
                            if (msgEntity.Type == MessageEntityType.Url)
                            {
                                //---------------------CHECK FORWARD?

                                var url = message.Text.Substring(msgEntity.Offset, msgEntity.Length);
                                try //a lot may go wrong when accessing urls
                                {
                                    if (IsImageUrl(url))
                                    {
                                        var bitmap = GetImageFromUrl(url);
                                        result = GetBayanResult(bitmap, message);
                                        if (result.IsBayan)
                                        {
                                            bayanResults.Add(result);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    return Ok();
                                }
                            }
                        }
                    }
                }
                //if photo is uploaded as a document, without compression
                else if (message?.Type == MessageType.DocumentMessage)
                {
                    if (IsImage(message.Document.MimeType))
                    {
                        var file = await _bot.GetFileAsync(message.Document.FileId);
                        var bitmap = new Bitmap(file.FileStream);
                        result = GetBayanResult(bitmap, message);
                    }
                }

                if (result.IsBayan || bayanResults.Count != 0)
                {


                    try
                    {
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: GetPunishMessage(), replyToMessageId: message.MessageId);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) //Message was deleted before resolve could happen, hence, no punishment
                    {
                        if (ex.Message == "Bad Request: message not found") { return Ok(); }
                    }
                    try
                    {
                        await Task.Delay(50);
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: $"Пруф. Инфа {Math.Round(result.Bayanity * 100)}%", replyToMessageId: result.OriginalImage.MessageId);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) //Message was deleted before resolve could happen
                    {
                        if (ex.Message == "Bad Request: message not found")
                        {

                            await _bot.SendTextMessageAsync(message.Chat.Id, text: GetProofForDeletedMessage(
                                GetUserTargetName(message.From),
                                result.OriginalImage.DateTimeAdded));
                        }
                    }
                    var achievMsg = GetAchievMessage(result.BayansCount, GetUserTargetName(message.From));
                    if (!string.IsNullOrEmpty(achievMsg))
                    {
                        await Task.Delay(50);
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: achievMsg, parseMode: ParseMode.Html);
                    }
                }

            }
            catch (Exception ex)
            {   //LOG NAHUY
                await _bot.SendTextMessageAsync(message.Chat.Id, text: "```" + ex.Message + "\n\n\n" + ex.StackTrace + "```", parseMode: ParseMode.Markdown);
            }
            return Ok();
        }

        [NonAction]
        public AntiBayanBot.Core.Models.BayanResult GetBayanResult(Bitmap bitmap, Telegram.Bot.Types.Message message)
        {
            Core.Models.MessageData forwardMsgData = null;

            if (message.ForwardFrom != null)
            {
                forwardMsgData = new Core.Models.MessageData()
                {
                    MessageId = message.MessageId,
                    ChatId = message.Chat.Id,
                    UserId = message.ForwardFrom.Id,
                    UserFullName = GetUserFullName(message.ForwardFrom.FirstName, message.ForwardFrom.LastName),
                    UserName = message.ForwardFrom.Username,
                    DateTimeAdded = message.ForwardDate.Value
                };
            }
            else if (message.ForwardFromChat != null)
            {
                forwardMsgData = new Core.Models.MessageData()
                {
                    MessageId = message.MessageId,
                    ChatId = message.Chat.Id,
                    UserId = message.ForwardFromChat.Id,
                    UserFullName = message.ForwardFromChat.Title,
                    UserName = message.ForwardFromChat.Title,
                    DateTimeAdded = message.ForwardDate.Value
                };
            }
            var msgData = new Core.Models.MessageData()
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                UserId = message.From.Id,
                UserFullName = GetUserFullName(message.From.FirstName, message.From.LastName),
                UserName = message.From.Username,
                DateTimeAdded = message.Date
            };

            var messageData = forwardMsgData ?? msgData;
            if (IsInnerForward(messageData.DateTimeAdded, messageData.UserId, messageData.ChatId))
            {
                return new Core.Models.BayanResult();
            }

            var result = Recognition.BayanDetector.DetectPhotoBayan(bitmap, messageData);
            if (result.IsBayan)
            {
                // Наказываем баяниста
                var statisticsRepository = new Core.Dal.StatisticsRepository();
                // Сколько он уже набаянил
                var bayans = statisticsRepository.IncrementBayansCount(msgData);
                result.BayansCount = bayans;
            }

            return result;
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
        public string GetAchievMessage(int bayanCount, string userName)
        {
            string achievMessage = null;
            if (bayanCount > 0)
            {
                var achievs = ConfigurationManager.AppSettings.AllKeys
                             .Where(key => key.StartsWith("Achiev"))
                             .Select(key =>
                             {
                                 var split = ConfigurationManager.AppSettings[key].Split('|');
                                 return new
                                 {
                                     Number = split[0],
                                     Name = split[1]
                                 };
                             })
                             .ToDictionary(num => Convert.ToInt32(num.Number), name => name.Name);
                foreach (var achiev in achievs)
                {
                    if (bayanCount == achiev.Key)
                    {
                        achievMessage = $"<b>\"{achiev.Value}\"</b>";
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(achievMessage))
            {
                achievMessage = $"🏆Пользователь @{userName} заработал достижение {achievMessage}🏆";
            }

            return achievMessage;
        }

        [NonAction]
        public string GetUserTargetName(Telegram.Bot.Types.User user)
        {
            return GetUserTargetName(user.Username, user.FirstName, user.LastName);
        }

        [NonAction]
        public string GetUserTargetName(string userName, string userFirstName, string userLastName = null)
        {
            return (string.IsNullOrEmpty(userName) ? GetUserFullName(userFirstName, userLastName) : userName).TrimEnd();
        }

        [NonAction]
        public string GetUserFullName(string userFirstName, string userLastName)
        {
            return (userFirstName + " " + userLastName).TrimEnd();
        }

        [NonAction]
        public string GetProofForDeletedMessage(string userName, DateTime dateTimeAdded)
        {
            var result = string.Format("@{0} запостил {1:yyyy-MM-dd} в {1:HH:mm} UTC, но за каким-то хером удалил.", userName, dateTimeAdded);
            return result;
        }

        [NonAction]
        public string GetStatsMessage(long chatId, string chatName)
        {
            var result = $"<b>Топ заядлых баянистов в группе {chatName}:</b>\n";
            var stats = new Core.Dal.StatisticsRepository().GetChatStatistics(chatId, 10);
            foreach (var user in stats)
            {
                result += $"{GetUserTargetName(user.UserName, user.UserFullName)} ({user.Bayans})\n";
            }

            return result;
        }

        [NonAction]
        public bool IsImageUrl(string url)
        {
            url = PrependUrl(url);
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/");
            }
        }

        [NonAction]
        public Bitmap GetImageFromUrl(string url)
        {
            url = PrependUrl(url);
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            using (var resp = req.GetResponse())
            {
                return new Bitmap(resp.GetResponseStream());
            }
        }

        /// <summary>
        /// Prepend url with http if it's missing
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [NonAction]
        public string PrependUrl(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }
            return url;
        }

        [NonAction]
        public bool IsInnerForward(DateTime? forwardDate, long? forwardFrom, long chatId)
        {
            if (forwardDate == null)
            {
                return false;
            }
            return new Core.Dal.ImageDataRepository().IsForwarded(forwardDate.Value, forwardFrom.Value, chatId);
        }

        [NonAction]
        public bool IsImage(string mimetype)
        {
            string[] mimetypes = new string[] { "image/png", "image/jpeg", "image/gif", "image/tiff", "image/x-tiff", "image/bmp", "image/x-windows-bmp", "image/x-icon" };
            return mimetypes.Contains(mimetype);
        }
    }
}
