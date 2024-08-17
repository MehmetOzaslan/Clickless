using Clickless.test;
using NUnit.Framework;

namespace NUnit.Tests
{
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
