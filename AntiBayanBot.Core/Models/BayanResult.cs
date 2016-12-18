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
        /// The measurment of image bayanity from 0 to 1 (percantage). 1 - full bayan.
        /// </summary>
        public float Bayanity { get; set; }

        /// <summary>
        /// NULL if image is original. Otherwise - point to original image from the DB.
        /// </summary>
        public ImageData OriginalImage { get; set; }
    }
}