namespace NUnit.Tests
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;
    using Clickless;
    using Clickless.src;
    using NUnit.Framework;

    [TestFixture]
    public class MouseTests
    {
        [Test]
        public void TestMovement()
        {
            for (int i = 0; i < 100; i+=10) {
                for (int j = 0; j < 100; j += 10)
                {
                    MouseController.MoveCursor(i, i);
                    var inf = MouseController.GetCursorInfo();
                    Assert.AreEqual(i, inf.ptScreenPos.x);
                    Assert.AreEqual(i, inf.ptScreenPos.y);
                }
            }
        }
    }

    [TestFixture]
    public class CaptureTests
    {
        [Test]
        public void TestCapture()
        {
            Image img = ScreenController.CaptureDesktop();
            Assert.That(img, Is.Not.Null);
            Assert.Greater(img.Width, 0);
            Assert.Greater(img.Height, 0);
        }

        [Test]
        public void TestCaptureSave()
        {
            Image img = ScreenController.CaptureDesktop("test.png");
            Assert.That(img, Is.Not.Null);
            Assert.Greater(img.Width, 0);
            Assert.Greater(img.Height, 0);
        }
    }
}