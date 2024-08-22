using System.Windows;

namespace Clickless
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Program.Run();
        }
    }
}
