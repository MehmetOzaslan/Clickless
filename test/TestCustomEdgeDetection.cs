using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using Clickless;
using NUnit.Framework;
namespace NUnit.Tests
{

    [TestFixture]
    public class EdgeDetectionTests 
    {

        private Bitmap bitmap;
        IEdgeProvider edgeDetecteCompute;

        [OneTimeSetUp]
        public void Setup()
        {
            bitmap = MonitorUtilities.CaptureDesktopBitmap();
            edgeDetecteCompute = new ImageRectDetectComputeShader();
        }

        [Test]
        [Repeat(10)]
        public void TestEdgeDetectionPerformance()
        {
            Stopwatch total_timer = Stopwatch.StartNew();
            IEnumerable ret = edgeDetecteCompute.GetEdges(bitmap);
            total_timer.Stop();
            Console.WriteLine("Total time took: " + total_timer.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void TestEdgeDetection()
        {
            IEnumerable ret = edgeDetecteCompute.GetEdges(bitmap);

            Assert.IsNotNull(ret);
            int count = 0;
            foreach (var edge in ret) { 
                count++;
            }

            Assert.AreNotEqual(0, count);

            Console.WriteLine("Image Size:" + 1920 * 1080);
            Console.WriteLine("Edge Pixel Count: " +  count);
        }
    }
}