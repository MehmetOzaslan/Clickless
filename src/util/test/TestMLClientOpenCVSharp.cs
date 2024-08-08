namespace NUnit.Tests
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clickless;
    using Clickless.src;
    using NUnit.Framework;
    using static Util.MathUtil;

    [TestFixture]
    public class TestMLClientOpenCVSharp
    {

        private Bitmap bitmap;
        IEdgeProvider edgeDetecteCompute;

        [OneTimeSetUp]
        public void Setup()
        {
            bitmap = ScreenController.CaptureDesktopBitmap();
            edgeDetecteCompute = new EdgeDetectOpenCVSharp();
        }

        [Test]
        public void TestConversion()
        {
            var mat = EdgeDetectOpenCVSharp.BitmapToMat(bitmap);

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
