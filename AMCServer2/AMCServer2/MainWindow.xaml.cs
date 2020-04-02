using System.Windows;

namespace AMCServer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the Datacontext of this View
            this.DataContext = new MainWindowViewModel();
            // Create the server backend
            ProgramState.ServerBackend = new Server();
        }
    }
}
