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
    }
}