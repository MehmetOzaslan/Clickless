using System;
using Clickless.src;
using Clickless.src.util.test;
using NUnit.Framework;
using static Clickless.src.WindowInfoRetriever;

namespace NUnit.Tests
{
    [TestFixture]
    public class TestWindowInfoRetriever
    {
        const string caption = "Test Caption";
        const string text = "Test Text";
        POINT point = new POINT(200, 200);

        [SetUp]
        public void Init()
        {
            WindowTestHelper.ShowMessage(text, caption, point);
        }

        [TearDown] 
        public void Cleanup() {
            WindowTestHelper.CloseAll();
        }


        [Test]
        public void TestCaption() {

            string windowText = GetWindowTextAtPoint(point);
            Assert.AreEqual(caption, windowText);
        }

        [Test]
        public void TestRect()
        {
            RECT windowRect = GetWindowRectAtPoint(point);
            Assert.AreEqual(WindowTestHelper.height, Math.Abs(windowRect.Top - windowRect.Bottom));
            Assert.AreEqual(WindowTestHelper.width, Math.Abs(windowRect.Right - windowRect.Left));
        }

        //NOTE: This may change if the window is made modal in the future.
        [Test]
        public void TestPID()
        {
            int PID = (int)GetWindowPIDAtPoint(point);
            int nProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
            Assert.AreEqual(nProcessID, PID);
        }

    }

}