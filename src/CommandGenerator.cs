using System;
using System.Collections.Generic;
using System.Linq;

namespace Clickless.src
{
    /// <summary>
    /// Given n objects, creates a distinct string starting with A, B, C, D... Z then AA, AB...
    /// </summary>
    /// 
    public static class CommandGenerator
    {
        private const int ASCII_A = 65;
        static readonly char[] chars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        //Every level you can add A, B, C ... D
        //The moment you go down one level, into say, A -> AA, then the previous layer is no longer valid.
        //Examples: 
        //1: BA -> BAA
        //2: A -> AA
        //3: ABC -> ABCD

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


        [Obsolete("No longer used in generation")]
        public static string int2cmd(int num)
        {
            string command = "";

            while (num > 0)
            {
                //Allow for proper indexing, eg: 1:A instead of 0:A
                num--;
                int remainder = num % 26;
                command = (char)(ASCII_A + remainder) + command;
                //Go to the next digit.
                num /= 26;
            }
            return command;
        }

        /// <summary>
        /// Creates n unique strings from [A..Z] 
        /// TODO: Ensure that all commands do not contain a prefix of another command.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<string> GenerateCommands(int num) {
            List<string> commands = ConstructCommands(num).ToList();
                

            return commands;
        }
    }
}
