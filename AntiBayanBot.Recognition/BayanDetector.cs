using AntiBayanBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntiBayanBot.Core;
using AntiBayanBot.Core.Dal;

namespace AntiBayanBot.Recognition
{
    public static class BayanDetector
    {
        /// <summary>
        /// Photo bayan detector method which uses OpenCV. NB: it saves image data to the DB, which is passed to it, if the image is not bayan.
        /// </summary>
        /// <param name="image">Image to check.</param>
        /// <param name="chatId">Chat ID as integer.</param>
        /// <param name="userId">User ID as integer.</param>
        /// <param name="datetimeAdded">Date and time of the message with image.</param>
        /// <returns>BayanResult object.</returns>
        public static BayanResult DetectPhotoBayan(Image image, int chatId, int userId, DateTime datetimeAdded)
        {
            var detector = new ImageFeatureDeteсtor();
            var repository = new ImageDataRepository();
            var similarityLimit = Settings.Get<float>("similarity");

            var descriptors = detector.GetDescriptors(image);
            var imagesData = repository.GetForChat(chatId);

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
                    return totalResult;
                }

                tasks.RemoveAt(i);
            }

            // If is not bayan, save the image data
            var newImageData = new ImageData(chatId, userId, datetimeAdded, descriptors);
            repository.Insert(newImageData);

            return totalResult;
        }
    }
}