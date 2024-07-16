using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NUnit.Tests
{
    using System.Linq;
    using Clickless.src;
    using NUnit.Framework;

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
        public void TestConversion()
        {
            Assert.AreEqual("A", CommandGenerator.int2cmd(1));
            Assert.AreEqual("B", CommandGenerator.int2cmd(2));
            Assert.AreEqual("C", CommandGenerator.int2cmd(3));
            Assert.AreEqual("D", CommandGenerator.int2cmd(4));
            Assert.AreEqual("E", CommandGenerator.int2cmd(5));
            Assert.AreEqual("F", CommandGenerator.int2cmd(6));
            Assert.AreEqual("Z", CommandGenerator.int2cmd(26));
            Assert.AreEqual("AA", CommandGenerator.int2cmd(27));
        }

        [Test]
        public void TestOrder()
        {
            int count = 20000;
            var cmds = CommandGenerator.GenerateCommands(count);
            Assert.AreEqual("A",cmds[0]);
            Assert.AreEqual("AA", cmds[26]);
            Assert.AreEqual("AAA", cmds[676+26]);
            Assert.AreEqual("AAAA", cmds[17576+676+26]);
        }

        [Test]
        public void TestLogCounting()
        {
            Assert.AreEqual(1, CommandGenerator.Log26Rounded(26));
            Assert.AreEqual(2, CommandGenerator.Log26Rounded(676));
            Assert.AreEqual(3, CommandGenerator.Log26Rounded(17576));
        }


    }
}