using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntiBayanBot.Core;
using AntiBayanBot.Core.Dal;
using AntiBayanBot.Core.Models;

namespace AntiBayanBot.Recognition
{
    public static class BayanDetector
    {
        /// <summary>
        /// Photo bayan detector method which uses OpenCV. NB: it saves image data to the DB, which is passed to it, if the image is not bayan.
        /// </summary>
        /// <param name="image">Image to check.</param>
        /// <param name="messageData">Message data.</param>
        /// <returns>BayanResult object.</returns>
        public static BayanResult DetectPhotoBayan(Image image, MessageData messageData)
        {
            var detector = new ImageFeatureDeteсtor();
            var repository = new ImageDataRepository();
            var similarityLimit = Settings.Get<float>("similarity");

            var descriptors = detector.GetDescriptors(image);
            var imagesData = repository.GetForChat(messageData.ChatId);

            var tasks = new List<Task<BayanResult>>(imagesData.Count);
            var cancellationTokenSource = new CancellationTokenSource();
            tasks.AddRange(imagesData.Select(imageData => Task.Factory.StartNew(() =>
            {
                var result = new BayanResult();

                var similarity = detector.GetSimilarity(descriptors, imageData.GetDescriptors());

                // Bayan detected
                if (similarity >= similarityLimit)
                {
                    result.IsBayan = true;
                    result.Bayanity = similarity;
                    result.OriginalImage = imageData;
                }

                return result;
            }, cancellationTokenSource.Token)));

            var totalResult = new BayanResult();
            while (tasks.Count > 0)
            {
                var i = Task.WaitAny(tasks.ToArray());

                var taskResult = tasks[i].Result;

                if (taskResult.IsBayan)
                {
                    // Cancell all tasks
                    cancellationTokenSource.Cancel();
                    totalResult = taskResult;

                    // Наказываем баяниста
                    var statisticsRepository = new StatisticsRepository();

                    // Сколько он уже набаянил
                    var bayans = statisticsRepository.IncrementBayansCount(messageData.ChatId, messageData.UserId);
                    taskResult.BayansCount = bayans;

                    return totalResult;
                }

                tasks.RemoveAt(i);
            }

            // If is not bayan, save the image data
            var newImageData = new ImageData(descriptors)
            {
                ChatId = messageData.ChatId,
                UserId = messageData.UserId,
                DateTimeAdded = messageData.DateTimeAdded,                
                MessageId = messageData.MessageId,
                UserFullName = messageData.UserFullName,
                UserName = messageData.UserName
            };
            repository.Insert(newImageData);

            return totalResult;
        }
    }
}