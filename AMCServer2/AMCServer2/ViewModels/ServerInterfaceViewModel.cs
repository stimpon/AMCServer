namespace AMCServer2
{
    using System;

    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System.Collections.ObjectModel;
    #endregion

    /// <summary>
    /// ViewModel for the ServerView
    /// </summary>
    public class ServerInterfaceViewModel : 
                 BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static ServerInterfaceViewModel VM { get; set; }

        /// <summary>
        /// These properties will be pulled from the
        /// server class that lives in the ProgramState
        /// class
        /// </summary>
        #region Server properties that should be exposed to the View

        /// <summary>
        /// Server listening port
        /// ---------------------------------------------------------
        /// this propery can only pull the data from the server class
        /// and not set it
        /// </summary>
        public int ListeningPort 
        {
            get => ProgramState.ServerBackend.ServerListeningPort;
            set { }
        }

        /// <summary>
        /// The current server state
        /// ---------------------------------------------------------
        /// this propery can only pull the data from the server class
        /// and not set it
        /// </summary>
        public ServerStates ServerState 
        { 
            get => ProgramState.ServerBackend.ServerState;
            set { }
        }

        /// <summary>
        /// Holds all of the connected clients
        /// ---------------------------------------------------------
        /// this propery can only pull the data from the server class
        /// and not set it
        /// </summary>
        public ObservableCollection<ClientViewModel> ActiveConnections 
        {
            get => ProgramState.ServerBackend.ActiveConnections;
            set { }
        }
        #endregion

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public ThreadSafeObservableCollection<LogItem> ServerLog { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServerInterfaceViewModel()
        {
            // Call the Init method when a new instance of this VM is created
            Init();
        }

        #region Functions

        /// <summary>
        /// First function that will be called when a new instance
        /// of this ViewModel is created
        /// </summary>
        private void Init()
        {
            // Set the accessable ViewModel to this class
            VM = this;

            // This is the standard message that shows when the program starts
            ServerLog = new ThreadSafeObservableCollection<LogItem>() { 
                new LogItem() { Content = "AMCServer [Version 1.0.0]", ShowTime = false } ,
                new LogItem() { Content = "(c) 2020 Stimpon",          ShowTime = false } ,
            };
        }

        #endregion

    }
}
