namespace AMCServer2
{
    /// <summary>
    /// Interaction logic for ServerView.xaml
    /// </summary>
    public partial class ServerView : 
                         BasePage<ServerInterfaceViewModel>
    {
        public ServerView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the page has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Focus on the console input box after the page has loaded
            ConsoleInput.Focus();
        }
    }
}
