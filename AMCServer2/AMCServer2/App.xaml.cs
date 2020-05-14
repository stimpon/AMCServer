using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AMCServer2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// Override the OnStartup method
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            ProgramState.IsRunning = true;

            // Call the base method
            base.OnStartup(e);

            // Setup the IoC
            IoC.DependencyInjectionCore.Configure();

            // Open the MainWindow
            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();
        }
    }
}
