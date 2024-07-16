﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Clickless.src.UI
{
    /// <summary>
    /// This represents the data to display on the screen when changes are made.
    /// </summary>
    internal class TextRect
    {
        //TODO: Move this to a settings file somewhere.
        static Brush borderBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Color.Magenta); // Changed to black for better visibility
        static Brush textBGBrush = new SolidBrush(Color.Black);
        static Brush textFGBrush = new SolidBrush(Color.White);
        public Font Font { get => font; set => font = value; }

        private Font font = new Font("Calibri", 14);


        public Rectangle Rectangle { get; set; }
        public string Text { get; set; }
        private int _textX {  get { return Rectangle.Width/2 + Rectangle.Left ; } }
        private int _textY { get { return Rectangle.Height/2 + Rectangle.Bottom; } }


        /// <summary>
        /// Draws a border with text in the center of the rect.
        /// </summary>
        /// <param name="g"></param>
        public void DrawToGraphics(Graphics g)
        {
            SizeF textSize = g.MeasureString(Text, Font) ;

            g.FillRectangle(textBGBrush, _textX, _textY, textSize.Width, textSize.Height);
            g.DrawString(Text, Font, textFGBrush, _textX, _textY);
            g.DrawRectangle(new Pen(borderBrush, 2), Rectangle);
        }

    }
}
