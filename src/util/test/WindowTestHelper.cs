using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clickless.src.util.test
{
    /// <summary>
    /// Helper class used to test window related operations 
    /// (EG: Clicking buttons, obtaining window information, rect sizes, etc.)
    /// </summary>
    class WindowTestHelper : Form
    {
        public const int width = 300;
        public const int height = 200;
        public readonly POINT defaultPos = new POINT(300, 300);

        private static Stack<WindowTestHelper> _windowTestHelpers = new Stack<WindowTestHelper>();
        public const string _text = "Test Text";
        private Label messageLabel;

        public WindowTestHelper()
        {
            this.messageLabel = new Label();
            this.messageLabel.AutoSize = true;
            this.messageLabel.Location = (Point) defaultPos;
            this.Controls.Add(messageLabel);

            this.Text = _text;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(width, height);
        }

        public static void ShowMessage(string message)
        {
            WindowTestHelper window = new WindowTestHelper();
            window.messageLabel.Text = message;
            window.Show();
            window.BringToFront();
        }

        public static void ShowMessage(string message, string caption)
        {
            WindowTestHelper window = new WindowTestHelper();
            window.messageLabel.Text = message;
            window.Text = caption;
            window.Show();
            window.BringToFront();
        }

        public static void ShowMessage(string message, string caption, POINT point)
        {
            WindowTestHelper window = new WindowTestHelper();
            window.messageLabel.Text = message;
            window.Text = caption;
            window.StartPosition = FormStartPosition.Manual;
            window.Location = (Point) point;
            window.Show();
            window.BringToFront();
        }


        public static void CloseMostRecent()
        {
            if (_windowTestHelpers == null) return;
            if (_windowTestHelpers.Count <= 0) return;

            var mostRecent = _windowTestHelpers.Pop();    
            mostRecent?.Close();
        }

        public static void CloseAll()
        {
            if (_windowTestHelpers == null) return;
            if (_windowTestHelpers.Count <= 0) return;

            while (_windowTestHelpers.Count > 0)
            {
                var mostRecent = _windowTestHelpers.Pop();
                mostRecent?.Close();
            }   
        }
    }
}
