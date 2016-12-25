﻿using System;
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

namespace AntiBayanBot.Web.Controllers
{
    public class AntiBayanBotController : ApiController
    {
        private readonly TelegramBotClient _bot = new TelegramBotClient(ConfigurationManager.AppSettings["AntiBayanBotToken"]);

        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;
            var result = new Core.Models.BayanResult();
            //For a case when single/multiple url-images w/ or w/o message text are in a single message. Contains only bayans
            var bayanResults = new List<Core.Models.BayanResult>();
            try
            {
                if (message?.Type == MessageType.PhotoMessage)
                {
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
                    if(message.Entities != null) { 
                        foreach(var msgEntity in message.Entities)
                        {
                            if(msgEntity.Type == MessageEntityType.Url)
                            {
                                var url = message.Text.Substring(msgEntity.Offset, msgEntity.Length);
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
                        }
                    }                    
                }
                //if photo is uploaded as a document, without compression
                else if (message?.Type == MessageType.DocumentMessage)
                {
                    if(message.Document.MimeType == "image/png")
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
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: "Пруф", replyToMessageId: result.OriginalImage.MessageId);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex) //Message was deleted before resolve could happen
                    {
                        if (ex.Message == "Bad Request: message not found") {

                            await _bot.SendTextMessageAsync(message.Chat.Id, text: GetProofForDeletedMessage(
                                GetUserTargetName(message.From),
                                result.OriginalImage.DateTimeAdded));
                        }
                    }
                    var achievMsg = GetAchievMessage(result.BayansCount, GetUserTargetName(message.From));
                    if (!string.IsNullOrEmpty(achievMsg))
                    {
                        await Task.Delay(50);
                        await _bot.SendTextMessageAsync(message.Chat.Id, text: achievMsg, parseMode: ParseMode.Markdown);
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
            var messageData = new Core.Models.MessageData
            {
                MessageId = message.MessageId,
                ChatId = message.Chat.Id,
                UserId = message.From.Id,
                DateTimeAdded = DateTime.UtcNow,
                UserFullName = GetUserFullName(message.From.FirstName, message.From.LastName),
                UserName = message.From.Username
            };

            return Recognition.BayanDetector.DetectPhotoBayan(bitmap, messageData);
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
                        achievMessage = $"*\"{achiev.Value}\"*";
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
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            using (var resp = req.GetResponse())
            {
                return new Bitmap(resp.GetResponseStream());
            }
        }
    }
}
 