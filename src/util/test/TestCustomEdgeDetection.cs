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
    using Clickless.src.edge;
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
            IEnumerable ret = edgeDetecteCompute.GetBitmapEdges(bitmap);

            Assert.IsNotNull(ret);
        }

    }

}