using System.Collections.Generic;

namespace Clickless.src
{
    /// <summary>
    /// Given n objects, creates a distinct string starting with A, B, C, D... Z then AA, AB...
    /// </summary>
    /// 
    public static class CommandGenerator
    {

        private const int ASCII_A = 65;

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
            List<string> commands = new List<string>();
            for (int i = 0; i < num; i++) {
                commands.Add(int2cmd(i + 1));
            }
            return commands;
        }
    }
}
