namespace AMCServer2
{
    /// <summary>
    /// ViewModel for the MainWindow
    /// </summary>
    public class MainWindowViewModel : 
                 BaseViewModel
    {

        #region Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static MainWindowViewModel VM { get; set; }

        /// <summary>
        /// Tells the View wich page to show
        /// </summary>
        public MainViews CurrentPage { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindowViewModel()
        {
            VM = this;
            CurrentPage = MainViews.ServerInterface;
        }
    }
}
