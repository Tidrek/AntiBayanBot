using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AntiBayanBot
{
    public class BayanDetector
    {
        public bool DetectPhotoBayan(Stream fileStream, int fileSize, string imageExt)
        {
            bool bayanDetected = false;

            if (imageExt.ToLower() == "gif")
            {
                //Compute md5
                string hash = ComputeHashFromStream(fileStream, fileSize);
                //Get existing hashes and compare
                //Set bayanDetected = true if hash is found 
            }
            else
            {
                //Get image features
                var bitmap = new Bitmap(fileStream);

                //Get existing features and compare
                //Set bayanDetected = true if chance is high            
            }

            return bayanDetected;
        }

        public string ComputeHashFromStream(Stream stream, int fileSize)
        {
            using (var md5 = MD5.Create())
            {
                byte[] buffer = null;
                stream.Read(buffer, 0, fileSize);
                return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "‌​").ToLower(); //standard looking md5              
            }
        }
    }
}
