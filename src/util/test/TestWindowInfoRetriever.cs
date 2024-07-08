using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NUnit.Tests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Clickless;
    using Clickless.src;
    using Clickless.src.util.test;
    using NUnit.Framework;
    using static Util.MathUtil;

    [TestFixture]
    public class TestWindowInfoRetriever
    {
        [Test]
        public void TestCaption() {
            const string caption = "Test Caption";
            const string text = "Test Text";
            Point point = new Point(200, 200);
            WindowTestHelper.ShowMessage(text, caption, point);
            string windowText = WindowInfoRetriever.GetWindowTextAtPoint(new MouseController.POINT(point.X, point.Y));
            Assert.AreEqual(caption, windowText);
        }

        [Test]
        public void TestRect()
        {
            const string caption = "Test Caption";
            const string text = "Test Text";
            Point point = new Point(200, 200);
            WindowTestHelper.ShowMessage(text, caption, point);
            RECT windowRect = WindowInfoRetriever.GetWindowRectAtPoint(new MouseController.POINT(point.X, point.Y));
            Assert.AreEqual(WindowTestHelper.height, Math.Abs(windowRect.Top - windowRect.Bottom));
            Assert.AreEqual(WindowTestHelper.width, Math.Abs(windowRect.Right - windowRect.Left));
        }

    }

}