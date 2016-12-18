using System.Diagnostics;
using System.Net.Mime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using AntiBayanBot.Recognition;
namespace AntiBayanBot.Tests
{
    [TestClass]
    public class UnitTest1
    {
        readonly string _coreDllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        [TestMethod]
        public void IsImage1AndImage2Similar()
        {
            var detector = new ImageFeatureDeteсtor();

            var img1 = detector.GetDescriptors(new Bitmap(_coreDllLocation + @"\Images\Image1.jpg"));
            var img2 = detector.GetDescriptors(new Bitmap(_coreDllLocation + @"\Images\Image2.jpg"));

            var result = detector.IsSimilar(img1, img2);

            Debug.WriteLine(result);
        }
    }
}