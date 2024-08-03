using System;
using System.Collections.Generic;
using System.Linq;

namespace Clickless.src
{
    /// <summary>
    /// Given n objects, creates a distinct string starting with A, B, C, D... Z then AB, AC, ensuring that prefixes aren't
    /// Contained.
    /// 
    /// Examples: 
    /// 1: BA -> BAA
    /// 2: A -> AA
    /// 3: ABC -> ABCD
    /// </summary>

    public static class CommandGenerator
    {
        private const int ASCII_A = 65;
        static readonly char[] chars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private static HashSet<string> ConstructCommands(int amount)
        {
            var prefixes = new List<string>();
            HashSet<string> commands = new HashSet<string>();

            Queue<(string, char)> prefixQueue = new Queue<(string, char)>();

            //Initialize the queue
            foreach (char ch in chars)
            {
                prefixQueue.Enqueue(("", ch));
            }

            while (commands.Count != amount)
            {
                var current = prefixQueue.Dequeue();
                var pre = current.Item1;
                var post = current.Item2;
                var combined = pre + post;

                //Remove the prefix.
                if (commands.Contains(pre)){
                    commands.Remove(pre);
                }
                //Add the new command.
                else
                {
                    commands.Add(combined);
                }
                
                //Add the next sequence of commands.
                foreach (char ch in chars)
                {
                    prefixQueue.Enqueue((combined, ch));
                }
            }

            prefixQueue.Clear();
            return commands;
        }

        /// <summary>
        /// Creates n unique strings from [A..Z] 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<string> GenerateCommands(int num) {
            List<string> commands = ConstructCommands(num).ToList();
            return commands;
        }
    }
}
