using Castle.Core.Logging;
using Medior.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Medior.Tests
{
    [TestClass]
    public class ManualTests
    {

        [TestMethod]
        public void Test()
        {
            // Do some manual tests here.

            var fileSystem = new Mock<IFileSystem>();
            var logger = new Mock<ILogger<ScreenGrabber>>();

            var screenGrabber = new ScreenGrabber(fileSystem.Object, logger.Object);
            var captureArea = Screen.PrimaryScreen.Bounds;

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < 120; i++)
            {
                screenGrabber.GetBitBltGrab(captureArea);
            }

            sw.Stop();

            Console.WriteLine($"BitBlt: {sw.Elapsed.TotalMilliseconds} ms");

            sw.Restart();

            for (var i = 0; i < 120; i++)
            {
                screenGrabber.GetWinFormsGrab(captureArea);
            }

            sw.Stop();

            Console.WriteLine($"WinForms: {sw.Elapsed.TotalMilliseconds} ms");
        }
    }
}
