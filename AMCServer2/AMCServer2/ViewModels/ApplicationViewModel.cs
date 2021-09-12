/// <summary>
/// Root namespace
/// </summary>
namespace AMCServer2
{
    #region Required namespaces
    using AMCServer;
    using AMCCore;
    #endregion

    /// <summary>
    /// The Application ViewModel
    /// </summary>
    public class ApplicationViewModel : BaseViewModel
    {
        #region Public properties

        /// <summary>
        /// Tells the View wich page to show
        /// </summary>
        public MainViews CurrentPage { get; set; }

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationViewModel()
        {
            // Set the CurrentPage to the ServerInterface
            CurrentPage = MainViews.ServerInterface;
        }

    }
}
