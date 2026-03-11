using RPM;
using System.Windows;

namespace RPM

{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Показываем окно авторизации при запуске
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}