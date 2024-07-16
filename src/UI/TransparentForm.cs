using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class TransparentForm : Form
{

    private string[] texts = { "A", "BB", "C", "DD", "E","F","G" };

    public TransparentForm()
    {
        this.BackColor = new Color();
        this.FormBorderStyle = FormBorderStyle.None;
        this.TransparencyKey = this.BackColor;
        this.Bounds = Screen.PrimaryScreen.Bounds;
        this.TopMost = true;
        this.DoubleBuffered = true;
    }




    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        Brush b = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Color.Magenta); // Changed to black for better visibility
        Brush t = new SolidBrush(Color.Black);
        Brush w = new SolidBrush(Color.White);

        int x = 100;
        int y = 100;

        foreach (var text in texts)
        {
            x += 30;
            y += 30;
        }
    }
}
