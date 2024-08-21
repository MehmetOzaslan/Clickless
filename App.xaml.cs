using Clickless.Views;
using System.Windows;

namespace Clickless
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Program.Run();
            //// Optionally, show the main window if needed
            //MainWindow mainWindow = new MainWindow();
            //mainWindow.Show();
        }
    }
}
