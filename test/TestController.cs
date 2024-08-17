namespace NUnit.Tests
{
    using System;
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
    public class MouseTests
    {
        [Test]
        public void TestMovement()
        {
            for (int i = 0; i < 100; i+=10) {
                for (int j = 0; j < 100; j += 10)
                {
                    MouseUtilities.MoveCursor(i, i);
                    var inf = MouseUtilities.GetCursorInfo();
                    Assert.AreEqual(i, inf.ptScreenPos.x);
                    Assert.AreEqual(i, inf.ptScreenPos.y);
                }
            }
        }

        [Test]
        public void TestIteration() {


            //Test random points.
            var pos = new[] { new Vector2(0, 0), new Vector2(10, 10), new Vector2(20, 20), new Vector2(11,11) };
            MouseUtilities.IterateOverLocations(pos, 
                (vec) => { Assert.AreNotEqual(MouseUtilities.GetCursorInfo().ptScreenPos.x, (int)vec.x); },
                (vec) => { Assert.AreEqual(MouseUtilities.GetCursorInfo().ptScreenPos.x, (int)vec.x); });
        }

        [Test]
        public void TestScreenIteration()
        {
            int error = 4;

            //Test the screen.
            var screenGrid = MonitorUtilities.ObtainGrid(10,10);
            MouseUtilities.IterateOverLocations(screenGrid,
                (vec) =>
                {
                    //ScreenController.CaptureSquare((int)vec.x,(int)vec.y,10);
                    Assert.AreNotEqual(vec.y, MouseUtilities.GetCursorInfo().ptScreenPos.y);
                },
                (vec) =>
                {
                    //ScreenController.CaptureSquare((int)vec.x, (int)vec.y, 10);
                    Assert.AreEqual(vec.y, MouseUtilities.GetCursorInfo().ptScreenPos.y, error);
                });
        }

        [Test]
        public void TestScreenIterationParallelCapture()
        {
            int error = 4;

            //Test the screen.
            var screenGrid = MonitorUtilities.ObtainGrid(10, 100).ToArray();
            
            MouseUtilities.IterateOverLocations(screenGrid,
                (vec) =>
                {
                    MonitorUtilities.CaptureSquare((int)vec.x,(int)vec.y, 32);
                },
                (vec) =>
                {
                    MonitorUtilities.CaptureSquare((int)vec.x, (int)vec.y, 32);
                },
                parallel: true
                );
        }
        [Test]
        public void TestScreenIterationParallel()
        {
            int error = 4;

            //Test the screen.
            var screenGrid = MonitorUtilities.ObtainGrid(10, 10);
            MouseUtilities.IterateOverLocations(screenGrid,
                (vec) =>
                {
                },
                (vec) =>
                {
                },
                parallel: true
                );
        }


        [Test]
        public void TestMouseStateCapture()
        {
            CursorStateTracker stateTracker = new CursorStateTracker();

            //Test the screen.
            var screenGrid = MonitorUtilities.ObtainGrid(10, 100);
            MouseUtilities.IterateOverLocations(screenGrid,
                (vec) =>
                {
                },
                (vec) =>
                {
                    var info = MouseUtilities.GetCursorInfo();
                    stateTracker.Update(vec, info);
                });

            Assert.GreaterOrEqual(stateTracker.GetPositionStates().Count, 10);
        }


        [Test]
        public void TestMouseImageSave()
        {
            CursorStateTracker stateTracker = new CursorStateTracker();

            //Test the screen.
            var screenGrid = MonitorUtilities.ObtainGrid(10, 10);
            MouseUtilities.IterateOverLocations(screenGrid,
                preAction: (vec) =>
                {
                },
                postAction: (vec) =>
                {
                    var info = MouseUtilities.GetCursorInfo();
                    stateTracker.Update(vec, info);
                },
                parallel : false
                );

            Assert.GreaterOrEqual(stateTracker.GetPositionStates().Count, 10);


            CursorImageGenerator generator = new CursorImageGenerator(stateTracker);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "images");

            generator.CreateImage(path + "testCapture.png");
        }


    }


    [TestFixture]
    public class CaptureTests
    {

        [Test]
        public void TestCapture()
        {
            Image img = MonitorUtilities.CaptureDesktop();
            Assert.That(img, Is.Not.Null);
                Assert.Greater(img.Width, 0);
                Assert.Greater(img.Height, 0);
        }

        [Test]
        public void TestCaptureSave()
        {
            Image img = MonitorUtilities.CaptureDesktop();
            MonitorUtilities.SaveImage(img);

            Assert.That(img, Is.Not.Null);
            Assert.Greater(img.Width, 0);
            Assert.Greater(img.Height, 0);
        }



        [Test]
        public void TestDesktopResolution()
        {
            var size = MonitorUtilities.GetSize();
            Assert.AreEqual(1920, size.Width);
            Assert.AreEqual(1080, size.Height);
        }

        [Test]
        public void TestCaptureSaveDesktopResolution()
        {
            Image img = MonitorUtilities.CaptureDesktop();
            MonitorUtilities.SaveImage(img);

            Assert.That(img, Is.Not.Null);
            Assert.AreEqual(1920, img.Width);
            Assert.AreEqual(1080,img.Height);
        }



        [Test]
        public void TestGrid()
        {
            var grid = MonitorUtilities.ObtainGrid(30,30);
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