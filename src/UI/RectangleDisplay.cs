using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Clickless;

public class RectangleDisplay : Form
{
    private string procName;
    public RectangleDisplay()
    {
        this.procName = Process.GetCurrentProcess().ProcessName;
        this.BackColor = new Color();
        this.FormBorderStyle = FormBorderStyle.None;
        this.TransparencyKey = this.BackColor;
        this.Bounds = Screen.PrimaryScreen.Bounds;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.Focus();
        this.LostFocus += new EventHandler((sender, e) => { ToFront(); } );
    }

    private void ToFront()
    {
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
        this.Focus();
        //FocusProcess(procName);
    }

    private List<TextRect> rects = new List<TextRect> { };
    
    public List<TextRect> Rects { get => rects; set { rects = value;  this.Invalidate(); } }

    public void SetRects(List<Rectangle> rects)
    {
        SetRects(TextRectGenerator.GenerateBoxesFromRects(rects));
    }

    public void SetRects(List<TextRect> rects)
    {
        this.rects = rects;
        Invalidate();
    }


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
