namespace NUnit.Tests
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clickless;
    using Clickless.src;
    using NUnit.Framework;
    using static Util.MathUtil;

    [TestFixture]
    public class EdgeDetectionTests
    {
        [Test]
        public void TestEdgeDetection()
        {
            EdgeDetectComputeShader edgeDetecteCompute = new EdgeDetectComputeShader();
            var bitmap = ScreenController.CaptureDesktopBitmap();
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