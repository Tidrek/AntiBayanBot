using System;

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
        /// User ID, who sent this image before. Null if is not bayan.
        /// </summary>
        public int? SentByUserId { get; set; }

        /// <summary>
        /// Date and time of sending the similar image. Null if is not bayan.
        /// </summary>
        public DateTime? SendAtDateTime { get; set; }
    }
}