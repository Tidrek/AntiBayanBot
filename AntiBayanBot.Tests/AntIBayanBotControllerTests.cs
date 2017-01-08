using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiBayanBot.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AntiBayanBot.Tests
{
    [TestClass]
    public class AntIBayanBotControllerTests
    {
        [TestMethod]
        public void PrependUrlWorks1()
        {
            var url = "google.com";
            var controller = new AntiBayanBotController();
            url = controller.PrependUrl(url);
            Assert.AreEqual(url, "http://google.com");
        }
        [TestMethod]
        public void PrependUrlWorks2()
        {
            var url = "http://google.com";
            var controller = new AntiBayanBotController();
            url = controller.PrependUrl(url);
            Assert.AreEqual(url, "http://google.com");
        }
        [TestMethod]
        public void PrependUrlWorks3()
        {
            var url = "https://google.com";
            var controller = new AntiBayanBotController();
            url = controller.PrependUrl(url);
            Assert.AreEqual(url, "https://google.com");
        }
    }
}
