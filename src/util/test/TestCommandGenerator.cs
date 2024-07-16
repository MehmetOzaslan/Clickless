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
    using Clickless;
    using Clickless.src;
    using NUnit.Framework;
    using static Util.MathUtil;

    [TestFixture]
    internal class TestCommandGenerator
    {
        [Test]
        public void TestGeneration()
        {
            int count = 1000;
            var cmds = CommandGenerator.GenerateCommands(count);
            Assert.AreEqual(count, cmds.Count);
        }

        [Test]
        public void TestUniqueness()
        {
            HashSet<string> strings = new HashSet<string>();
            int count = 1000;
            var cmds = CommandGenerator.GenerateCommands(count);
            var unique = cmds.Select(x => x).Distinct();

            Assert.AreEqual(count, cmds.Count);
            Assert.AreEqual(count, unique.Count());
        }

        [Test]
        public void TestOrder()
        {
            int count = 20000;
            var cmds = CommandGenerator.GenerateCommands(count);
            Assert.AreEqual("A",cmds[0]);
            Assert.AreEqual("AA", cmds[26]);
            Assert.AreEqual("AAA", cmds[676]);
            Assert.AreEqual("AAAA", cmds[17576]);
        }


    }
}