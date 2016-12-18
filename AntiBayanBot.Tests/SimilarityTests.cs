using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using System.Reflection;
using AntiBayanBot.Recognition;
namespace AntiBayanBot.Tests
{
    [TestClass]
    public class SimilarityTests
    {
        private readonly string _dllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        private readonly ImageFeatureDeteсtor _detector = new ImageFeatureDeteсtor();

        [TestMethod]
        public void IsImage1AndImage2Similar()
        {
            var img1 = _detector.GetDescriptors(Image.FromFile(_dllLocation + @"\Images\Image1.jpg"));
            var img2 = _detector.GetDescriptors(Image.FromFile(_dllLocation + @"\Images\Image2.jpg"));

            var result = _detector.GetSimilarity(img1, img2);

            Assert.IsTrue(result >= 0.65);

            Debug.WriteLine(result);
        }


        [TestMethod]
        public void IsImage1AndImage3Different()
        {
            var detector = new ImageFeatureDeteсtor();

            var img1 = detector.GetDescriptors(Image.FromFile(_dllLocation + @"\Images\Image1.jpg"));
            var img2 = detector.GetDescriptors(Image.FromFile(_dllLocation + @"\Images\Image3.jpg"));

            var result = detector.GetSimilarity(img1, img2);

            Assert.IsTrue(result < 0.65);

            Debug.WriteLine(result);
        }
    }
}