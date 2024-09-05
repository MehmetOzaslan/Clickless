using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Clickless;

public class RectangleDisplay : Form
{
    public RectangleDisplay()
    {
        this.BackColor = new Color();
        this.FormBorderStyle = FormBorderStyle.None;
        this.TransparencyKey = this.BackColor;
        this.Bounds = Screen.PrimaryScreen.Bounds;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.LostFocus += new EventHandler((sender, e) => { ToFront(); } );
    }


    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr WindowHandle);
    public const int SW_RESTORE = 9;
    private void FocusProcess(string procName)
    {
        Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);
        if (objProcesses.Length > 0)
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = objProcesses[0].MainWindowHandle;
            ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
            SetForegroundWindow(objProcesses[0].MainWindowHandle);
        }
    }

    private void ToFront()
    {
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
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
