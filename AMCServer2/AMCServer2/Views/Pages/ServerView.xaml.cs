#region Required namespaces
using System.Collections.Specialized;
#endregion

/// <summary>
/// Root namespace
/// </summary>
namespace AMCServer2
{
    /// <summary>
    /// Interaction logic for ServerView.xaml
    /// </summary>
    public partial class ServerView : 
                         BasePage<ServerInterfaceViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerView"/> class.
        /// </summary>
        public ServerView()
        {
            // Initialize
            Initialize();
        }

        #region Private functions

        private void Initialize()
        {
            // Initialize all components (Standard procedure)
            InitializeComponent();

            // Create event for when an item is added to the server terminal
            ((INotifyCollectionChanged)ServerTerminal.Items).CollectionChanged 
                += ServerView_CollectionChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the CollectionChanged event of the ServerView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ServerView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If an item was added
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                // Scroll to the bottom of the listview
                ServerTerminal.ScrollIntoView(e.NewItems[0]);
            }
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

        #endregion

    }
}
