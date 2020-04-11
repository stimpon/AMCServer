using System.Windows;

namespace AMCClient2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Override the base OnStartup method
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            ProgramState.IsRunning = true;

            // Call the base method
            base.OnStartup(e);

            // Setup the IoC
            IoC.Container.SetupIoC();

            // Show the MainWindow
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
        }

    }
}
