namespace AntiBayanBot.Core.Models
{
    /// <summary>
    /// The result of check for bayan.
    /// </summary>
    public class BayanResult
    {
        /// <summary>
        /// True if is bayan.
        /// </summary>
        public bool IsBayan { get; set; }

        /// <summary>
        /// The measurment of image bayanity from 0 to 1 (percentage). (1 - full bayan).
        /// </summary>
        public float Bayanity { get; set; }

        /// <summary>
        /// How many bayans user posted to this chat (chat with ID = OriginalImage.ChatId)
        /// </summary>
        public int BayansCount { get; set; }

        /// <summary>
        /// NULL if image is original. Otherwise - pointer to original image from the DB.
        /// </summary>
        public ImageData OriginalImage { get; set; }
    }
}