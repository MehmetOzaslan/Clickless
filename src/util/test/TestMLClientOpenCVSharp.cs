namespace NUnit.Tests
{
    using System;
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
        [Test]
        public void TestConversion()
        {
            var bitmap =  ScreenController.CaptureDesktopBitmap();
            var mat = EdgeDetectOpenCVSharp.BitmapToMat(bitmap);

            Assert.IsTrue( mat.SaveImage("Conversion From Bitmap to OpenCV Mat.png"));
            bitmap.Dispose();
        }

        [Test]
        public void TestCapture()
        {
            var bitmap = ScreenController.CaptureDesktopBitmap();
            var bboxes = MLClient.GetBboxes(bitmap);
            Assert.GreaterOrEqual(1, bboxes.Count);
        }
    }
}
