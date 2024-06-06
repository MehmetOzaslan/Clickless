namespace NUnit.Tests
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Clickless;
    using Clickless.src;
    using NUnit.Framework;
    using static Util.MathUtil;

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

        [Test]
        public void TestIteration() {


            //Test random points.
            var pos = new[] { new Vector2(0, 0), new Vector2(10, 10), new Vector2(20, 20), new Vector2(11,11) };
            MouseController.IterateOverLocations(pos, 
                (vec) => { Assert.AreNotEqual(MouseController.GetCursorInfo().ptScreenPos.x, (int)vec.x); },
                (vec) => { Assert.AreEqual(MouseController.GetCursorInfo().ptScreenPos.x, (int)vec.x); });
        }

        [Test]
        public void TestScreenIteration()
        {
            int error = 4;

            //Test the screen.
            var screenGrid = ScreenController.ObtainGrid(10,10);
            MouseController.IterateOverLocations(screenGrid,
                (vec) => {
                    Assert.AreNotEqual(vec.y, MouseController.GetCursorInfo().ptScreenPos.y);
                },
                (vec) => {
                    Assert.AreEqual(vec.y, MouseController.GetCursorInfo().ptScreenPos.y, error);
                });
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


        [Test]
        public void TestGrid()
        {
            var grid = ScreenController.ObtainGrid(30,30);
            Assert.That(grid, Is.Not.Null);
            
            //Ensure there are actually elements in here.
            Assert.That(grid.Count, Is.GreaterThan(0));

            //Test the padding
            Assert.That(grid[0].x, Is.EqualTo(30));
            Assert.That(grid[0].y, Is.EqualTo(30));

            //Test the spacing
            Assert.AreEqual(30, grid[0].y, 2);
            Assert.AreEqual(60, grid[1].y, 2);
            Assert.AreEqual(90, grid[2].y, 2);
        }
    }
}