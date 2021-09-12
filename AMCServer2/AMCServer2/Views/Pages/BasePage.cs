namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System.Windows.Controls;
    using AMCCore;
    #endregion

    /// <summary>
    /// Generic class that replaces the Page class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasePage<T> : Page
        where T: BaseViewModel, new() {

        #region Property of this ViewModel

        /// <summary>
        /// Private instance of this ViewModel
        /// </summary>
        private T viewModel;
        /// <summary>
        /// Public property of this ViewModel
        /// </summary>
        public T ViewModel { get => viewModel; set { if (viewModel != value) viewModel = value; this.DataContext = value; } }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public BasePage()
        {
            // Set the ViewModel of this Page When it is created
            this.ViewModel = new T();
        }

    }
}
