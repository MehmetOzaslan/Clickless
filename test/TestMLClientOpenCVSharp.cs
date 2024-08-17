using System;
using System.Diagnostics;
using System.Drawing;
using Clickless;
using NUnit.Framework;
namespace NUnit.Tests
{

    [TestFixture]
    public class TestMLClientOpenCVSharp
    {

        private Bitmap bitmap;
        IEdgeProvider edgeDetecteCompute;

        [OneTimeSetUp]
        public void Setup()
        {
            bitmap = MonitorUtilities.CaptureDesktopBitmap();
            edgeDetecteCompute = new ImageRectDetectOpenCVSharp();
        }

        [Test]
        public void TestConversion()
        {
            var mat = ImageRectDetectOpenCVSharp.BitmapToMat(bitmap);

            Assert.IsTrue( mat.SaveImage("Conversion From Bitmap to OpenCV Mat.png"));
            bitmap.Dispose();
        }

        [Test]
        public void TestEdgeDetection()
        {
            Stopwatch total_timer = Stopwatch.StartNew();
            var bboxes = edgeDetecteCompute.GetEdges(bitmap);
            Console.WriteLine("Total time took: " + total_timer.Elapsed.TotalMilliseconds);
        }

        [Test]
        [Repeat(10)]
        public void TestEdgeDetectionPerformance()
        {
            Stopwatch total_timer = Stopwatch.StartNew();
            var bboxes = edgeDetecteCompute.GetEdges(bitmap);
            Console.WriteLine("Total time took: " + total_timer.Elapsed.TotalMilliseconds);
        }
    }
}
