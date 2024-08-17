using System.Drawing;
using System.Drawing.Drawing2D;

namespace Clickless
{
    /// <summary>
    /// This represents the data to display on the screen when changes are made.
    /// </summary>
    public class TextRect
    {
        public TextRect(Rectangle rect, string text){
            this.Rectangle = rect;
            this.Text = text;
        }

        static Brush borderBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, ColorTranslator.FromHtml("#219ebc"));
        static Brush textBGBorderBrush = new SolidBrush(ColorTranslator.FromHtml("#ffb703"));
        static Brush textBGBrush = new SolidBrush(ColorTranslator.FromHtml("#023047"));
        static Brush textFGBrush = new SolidBrush(ColorTranslator.FromHtml("#8ecae6"));
        public Font Font { get => font; set => font = value; }

        private Font font = new Font("Calibri", 12);


        public Rectangle Rectangle { get; set; }
        public string Text { get; set; }
        private int _textX { get { return Rectangle.Left + Rectangle.Width / 2; } }
        private int _textY { get { return Rectangle.Top + Rectangle.Height / 2; } }


        /// <summary>
        /// Draws a border with text in the center of the rect.
        /// </summary>
        /// <param name="g"></param>
        public void DrawToGraphics(Graphics g)
        {
            SizeF textSize = g.MeasureString(Text, Font) ;

            g.FillRectangle(textBGBorderBrush, _textX-2, _textY-2, textSize.Width+4, textSize.Height+4);
            g.FillRectangle(textBGBrush, _textX, _textY, textSize.Width, textSize.Height);
            g.DrawString(Text, Font, textFGBrush, _textX, _textY);
            g.DrawRectangle(new Pen(borderBrush, 2), Rectangle);
        }

    }
}
