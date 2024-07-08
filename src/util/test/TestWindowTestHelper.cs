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
    public class TestWindowTestHelper
    {
        [Test]
        public void TestOne()
        {
            WindowTestHelper.ShowMessage("Test One");
        }

        [Test]
        public void TestMany()
        {
            for (int i = 1; i < 10; i++)
            {
                WindowTestHelper.ShowMessage("Test " + i );
            }
            WindowTestHelper.CloseAll();
        }


    }

}
