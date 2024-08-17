using System.Collections.Generic;
using System.Linq;
using Clickless;
using NUnit.Framework;

namespace NUnit.Tests
{

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