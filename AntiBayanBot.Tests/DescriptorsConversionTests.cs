using AntiBayanBot.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AntiBayanBot.Tests
{
    [TestClass]
    public class DescriptorsConversionTests
    {
        [TestMethod]
        public void ConversionToBytesIsCorerct()
        {
            const int x = 2;
            var matrix = new float[x, 64];

            var counter = 0;
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < 64; j++)
                {
                    matrix[i, j] = counter;
                    counter++;
                }
            }

            var imageData = new ImageData();
            imageData.SetDescriptors(matrix);

            var computedMatrix = imageData.GetDescriptors();

            Assert.AreEqual(0, computedMatrix[0,0]);
            Assert.AreEqual(x*64-1, computedMatrix[x-1,64-1]);
        }
    }
}