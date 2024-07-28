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
using static System.Net.Mime.MediaTypeNames;
using static Clickless.src.KeyboardHook;

namespace Clickless.src
{
    /// <summary>
    /// Converts the json response through the websocket to a text rect.
    /// </summary>


    public class TextRectGenerator
    {
        private List<TextRect> _bboxes = new List<TextRect>();
        public event EventHandler Emptied;
        public event EventHandler Filtered;
        public event EventHandler Filled;


        public void SetBoxes(List<TextRect> bboxes) {  
            _bboxes = bboxes;
            Filled?.Invoke(this, EventArgs.Empty);
        }

        public void SetBoxesFromResponse(string response)
        {
            SetBoxes(GenerateBoxesFromResponse(response));   
        }

        //Filter by the first character for each rect.
        public void FilterBoxes(char filter) {
            if (!checkBoxFull())
            {
                Emptied?.Invoke(this, EventArgs.Empty);
                return;
            }

            //Filter the boxes.
            _bboxes = _bboxes.Where(x=> x.Text[0] == filter).ToList();


            if (!checkBoxFull())
            {
                Emptied?.Invoke(this, EventArgs.Empty);
                return;
            }

            else
            {
                Filtered?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearBoxes()
        {
            _bboxes.Clear();
            Emptied?.Invoke(this, EventArgs.Empty);
        }

        private bool checkBoxFull()
        {
            return _bboxes != null &&  _bboxes.Count > 0;
        }


        public static List<TextRect> GenerateBoxesFromResponse(string json_response)
        {
            List<TextRect> bboxes = new List<TextRect>();
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

            //Wrap them together in the text rect list.
            for (int i = 0; i < commands.Count; i++)
            {
                TextRect textRect = new TextRect(rects[i], commands[i]);
                bboxes.Add(textRect);
            }

            return bboxes;
        }



    }
}
