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
        
        #region Commands

        /// <summary>
        /// When a command is exeuted in the terminal
        /// </summary>
        public RelayCommand ExecuteCommand { get; set; }

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

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand = new RelayCommand(ExecuteCommandEvent);

            #endregion

        }

        /// <summary>
        /// Actions for the commands
        /// </summary>
        #region Command actions

        /// <summary>
        /// Bound to the ExecuteCommand
        /// </summary>
        private void ExecuteCommandEvent(object parameter)
        {
            // We now that the parameter will be a string
            string Command = parameter as string;
        }

        #endregion

        #endregion

    }
}
