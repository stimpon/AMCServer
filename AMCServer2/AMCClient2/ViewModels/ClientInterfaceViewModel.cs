﻿namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    #endregion

    /// <summary>
    /// ViewModel for the Client Interface
    /// </summary>
    public class ClientInterfaceViewModel : BaseViewModel
    {

        #region Public Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static ClientInterfaceViewModel VM { get; set; }

        #region Commands

        /// <summary>
        /// When a command is exeuted in the terminal
        /// </summary>
        public RelayCommandNoParameters ExecuteCommand { get; set; }

        #endregion

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public ThreadSafeObservableCollection<LogItem> ServerLog { get; set; }

        /// <summary>
        /// The string that is linked to the command box
        /// </summary>
        public string CommandString
        {
            get;
            set;
        } = String.Empty;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientInterfaceViewModel()
        {
            if (ProgramState.IsRunning)
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
                new LogItem() { Content = "AMCClient [Version 1.0.0]", ShowTime = false, Type = InformationTypes.Information } ,
                new LogItem() { Content = "(c) 2020 Stimpon",          ShowTime = false, Type = InformationTypes.Information } ,
            };

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand = new RelayCommandNoParameters(ExecuteCommandEvent);

            #endregion

            // Subscribe to server events
            IoC.Container.Get<ClientViewModel>().ClientInformation += OnClientInformation;

        }

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        private void ResolveCommand()
        {
            // Check the command
            switch (CommandString.ToLower())
            {
                // :connect >> Connect to the server
                case ":connect": IoC.Container.Get<ClientViewModel>().Connect(); break;

                // Invalid command
                default:
                    ServerLog.Add(new LogItem() { Content = $"'{CommandString}' is not recognized as a command, use ':h' for help", ShowTime = false });
                    return;
            }
        }

        /// <summary>
        /// When a message is sent from the server, print it to the terminal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientInformation(object sender, InformationEventArgs e)
        {
            ServerLog.Add(new LogItem()
            {
                Content = e.Information,
                EventTime = DateTime.Now.ToString(),
                ShowTime = true,
                Type = e.MessageType
            });
        }

        /// <summary>
        /// Actions for the commands
        /// </summary>
        #region Command actions

        /// <summary>
        /// Bound to the ExecuteCommand
        /// </summary>
        private void ExecuteCommandEvent()
        {
            // Return is command is null or empty
            if (String.IsNullOrEmpty(CommandString)) return;

            // Don't hande the command resolve action here
            ResolveCommand();

            // Clear the Input line
            CommandString = String.Empty;
        }

        #endregion

        #endregion

    }
}
