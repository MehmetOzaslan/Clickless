using Clickless.src.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class TransparentForm : Form
{
    public TransparentForm()
    {
        this.BackColor = new Color();
        this.FormBorderStyle = FormBorderStyle.None;
        this.TransparencyKey = this.BackColor;
        this.Bounds = Screen.PrimaryScreen.Bounds;
        this.TopMost = true;
        this.DoubleBuffered = true;

        this.LostFocus += new EventHandler((sender, e) => { toFront(); } );
    }

    
    private void toFront()
    {
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    private List<TextRect> rects = new List<TextRect> { };

    public List<TextRect> Rects { get => rects; set { rects = value;  this.Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        foreach (var rect in rects)
        {
            rect.DrawToGraphics(g);
        }
    }
}
