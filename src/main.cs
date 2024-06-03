using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.src
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            Console.WriteLine(args.Length);
            MouseController.MoveCursor(1, 1);
        }
    }

}
