namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    #endregion

    public class ApplicationViewModel : BaseViewModel
    {
        #region Public properties

        /// <summary>
        /// Tells the View wich page to show
        /// </summary>
        public MainViews CurrentPage         { get; private set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationViewModel()
        {
            // Set the CurrentPage to the ServerInterface
            CurrentPage = MainViews.ClientInterface;
        }
    }
}
