using System;
namespace AntiBayanBot.Core.Models
{
    public class ImageData : MessageData
    {
        public byte[] Descriptors { get; set; }

        public void SetDescriptors(float[,] data)
        {
            var flatMatrix = new float[data.Length];

            // Flat matrix
            var i = 0;
            foreach (var value in data)
            {
                flatMatrix[i] = value;
                i++;
            }

            var byteArray = new byte[flatMatrix.Length * 4];
            Buffer.BlockCopy(flatMatrix, 0, byteArray, 0, byteArray.Length);

            Descriptors = byteArray;
        }

        public float[,] GetDescriptors()
        {
            var floatArray = new float[Descriptors.Length / 4];
            Buffer.BlockCopy(Descriptors, 0, floatArray, 0, Descriptors.Length);

            var rows = floatArray.Length/64;

            var matrix = new float[rows, 64];
            var row = 0;
            var column = 0;
            foreach (var value in floatArray)
            {
                matrix[row, column] = value;

                column++;

                if (column == 64)
                {
                    column = 0;
                    row++;
                }
            }

            return matrix;
        }

        public ImageData() { }

        public ImageData(float[,] descriptors)
        {
            SetDescriptors(descriptors);
        }
    }
}