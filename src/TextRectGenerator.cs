using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using Clickless.src.UI;
using System.Linq;

namespace Clickless.src
{
    /// <summary>
    /// Zips rects and commands together.
    /// </summary>
    /// 
    public class TextRectGenerator
    {
        public static List<TextRect> GenerateBoxesFromRects(List<Rectangle> rects)
        {
            //Create the text commands.
            var commands = CommandGenerator.GenerateCommands(rects.Count);

            //Zip them together.
            return rects.Zip(commands, (rect, command) => new TextRect(rect, command)).ToList();
        }

        //Follow the format:
        //[[xmin, ymin, xmax, ymax],[xmin, ymin, xmax, ymax],[xmin, ymin, xmax, ymax]]
        [Obsolete("Latency over websockets is too large, deprecated to pivot to keeping everything on the client.")]
        public static List<TextRect> GenerateBoxesFromResponse(string json_response)
        {

            //Obtain all integers in the bounding rects.
            string[] number_strs = Regex.Split(json_response, @"\D+");

            List<int> numbers = new List<int>();
            foreach (string value in number_strs)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    int i = int.Parse(value);
                    numbers.Add(i);
                }
            }

            if (numbers.Count % 4 != 0)
            {
                throw new Exception("Generated bounding rects failed to be parsed, incorrect format.");
            }

            //Create all rects.
            List<Rectangle> rects = new List<Rectangle>();

            for (int i = 0; i < numbers.Count; i+=4)
            {

                int xmin = numbers[i];
                int ymin = numbers[i + 1];
                int xmax = numbers[i + 2];
                int ymax = numbers[i + 3];

                Rectangle rect = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
                rects.Add(rect);
            }
            return GenerateBoxesFromRects(rects);
        }
    }
}
