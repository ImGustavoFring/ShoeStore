using System.Windows;
using ShoeStore.WpfApp.Views;  // предположим, окна будут в папке Views

namespace ShoeStore.WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}