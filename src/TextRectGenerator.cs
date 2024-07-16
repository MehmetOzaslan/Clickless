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
        /// Gets the logarithm of a number in base 26. Divides until the number is > 26.
        /// </summary>
        private static int Log26Rounded(int num)
        {
            int count = 0;
            while (num > 26)
            {
                num/= 26;
                count++;
            }
            return count;
        }


        public static List<string> GenerateCommands(int num) {
            List<string> commands = new List<string>();
            
            int[] ints = new int[num];

            int _currNum = 0;
            for (int i = 0; i < num; i++) {
                ints[i] = i;
            }

            //We are basically counting with base 26.
            //26
            //676
            //17576

            foreach (int i in ints) {

                //Number of digits obtained through logarithm.
                int digitCount = Log26Rounded(i);
                int str_length = 0;

                var command = "";
                do
                {
                    if(digitCount == 0)
                    {
                        command += (char)((i) + ASCII_A);
                    }
                    else
                    {
                        command += (char)(i/(26*digitCount) + ASCII_A);
                    }
                    str_length += 1;
                } while (str_length < digitCount);
                commands.Add(command);
            }


            return commands;
        }
    }

    internal class TextRectGenerator
    {



    }
}
