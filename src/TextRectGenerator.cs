using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.src
{
    /// <summary>
    /// Given n objects, creates a distinct string starting with A, B, C, D... Z then AA, AB...
    /// </summary>
    /// 
    public static class CommandGenerator
    {
        public const int ASCII_A = 65;

        /// <summary>
        /// Gets the logarithm of a number in base 26. 
        /// Multiplies 1 until it's greater than the number.
        /// </summary>
        public static int Log26Rounded(int num)
        {
            int count = 0;
            int counter = 1;
            do
            {
                counter *= 26;
                count++;
            } while (counter < num);

            return count;
        }

        public static string int2cmd(int num)
        {
            string command = "";

            while (num > 0)
            {
                num--;
                int remainder = num % 26;
                command = (char)(ASCII_A + remainder) + command;
                //Go to the next digit.
                num /= 26;
            }

            return command;
        }

        public static List<string> GenerateCommands(int num) {
            List<string> commands = new List<string>();
            
            int[] ints = new int[num];
            
            int _currNum = 0;
            for (int i = 1; i < num; i++) {
                commands.Add(int2cmd(i));
            }

            return commands;
        }
    }

    internal class TextRectGenerator
    {



    }
}
