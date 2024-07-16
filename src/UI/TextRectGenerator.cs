using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using Clickless.src.UI;

namespace Clickless.src
{
    /// <summary>
    /// Converts the json response through the websocket to a text rect.
    /// </summary>
    ///


    internal class TextRectGenerator
    {

        
        public static List<TextRect> GenerateBoxes(string json_response)
        {
            //I could parse the response into a json and add a incredibly large library.
            //Or I could just follow the following format:
            //[[xmin, ymin, xmax, ymax],[xmin, ymin, xmax, ymax],[xmin, ymin, xmax, ymax]]

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

            //Create the text commands.
            var commands = CommandGenerator.GenerateCommands(rects.Count);


            //Wrap them together in the text rect object.
            List<TextRect> textRects = new List<TextRect>();
            for (int i = 0; i < commands.Count; i++)
            {
                TextRect textRect = new TextRect(rects[i], commands[i]);
                textRects.Add(textRect);
            }

            return textRects;
        }



    }
}
