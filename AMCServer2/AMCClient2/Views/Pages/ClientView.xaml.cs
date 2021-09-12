/// <summary>
/// Root namespace
/// </summary>
namespace AMCClient2
{
    #region Required namespaces
    using System.Collections.Specialized;
    using System.Windows;
    #endregion

    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class ClientView : BasePage<ClientInterfaceViewModel>
    {
        #region Default constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientView"/> class.
        /// </summary>
        public ClientView()
        {
            // Initialize
            Initialize();
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            // Initialize components
            InitializeComponent();

            // Create event for when an item is added to ther client terminal
            ((INotifyCollectionChanged)ClientTerminal.Items).CollectionChanged 
                += ClientView_CollectionChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the CollectionChanged event of the ClientView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ClientView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If an item was added to the client terminal
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                // Scroll to the bottom of the terminal
                ClientTerminal.ScrollIntoView(e.NewItems[0]);
            }
        }

        /// <summary>
        /// Fires when the page has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on the console input box when the page is loaded
            ConsoleInput.Focus();
        }

        #endregion
    }
}
