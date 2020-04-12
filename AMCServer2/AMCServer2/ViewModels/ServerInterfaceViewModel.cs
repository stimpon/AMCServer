namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.Windows;
    #endregion

    /// <summary>
    /// ViewModel for the ServerView
    /// </summary>
    public class ServerInterfaceViewModel : 
                 BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static ServerInterfaceViewModel VM { get; set; }
        
        #region Commands

        /// <summary>
        /// When a command is exeuted in the terminal
        /// </summary>
        public RelayCommandNoParameters ExecuteCommand { get; set; }

        public RelayCommand ExplorerDoubleClick { get; set; }

        #endregion

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public ThreadSafeObservableCollection<LogItem> ServerLog { get; set; }

        /// <summary>
        /// These are the items that will be displayed in the explorer
        /// </summary>
        public ThreadSafeObservableCollection<FileExplorerObject> ExplorerItems { get; set; }

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
        public ServerInterfaceViewModel()
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
                new LogItem() { Content = "AMCServer [Version 1.0.0]", ShowTime = false, Type = InformationTypes.Information } ,
                new LogItem() { Content = "(c) 2020 Stimpon",          ShowTime = false, Type = InformationTypes.Information } ,
            };

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand = new RelayCommandNoParameters(ExecuteCommandEvent);
            // Navigate command
            ExplorerDoubleClick = new RelayCommand(NavigateEvent, (o) => { return true; });

            #endregion

            // Subscribe to server events
            IoC.Container.Get<ServerViewModel>().ServerInformation += OnServerInformation;
            IoC.Container.Get<ServerViewModel>().DataReceived += OnDataReceived;

        }

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        private void ResolveCommand()
        {
            // Check if input command

            // :bind + [ID] >> Bind the server to a client
            if(CommandString.ToLower().StartsWith("bind "))
            {
                // Try to parse the argument into an int
                if (int.TryParse(CommandString.Substring(5), out int ID))
                    IoC.Container.Get<ServerViewModel>().Bind(ID);
                else
                    ServerLog.Add(new LogItem() { Content  = $"'{CommandString.Substring(5)}' is an invalid ID", 
                                                  ShowTime = false, 
                                                  Type     = InformationTypes.Error });
            }

            // Check if standalone command
            else
            {
                switch (CommandString.ToLower())
                {
                    /* :cls >> Clear the console
                     */
                    case ":cls": ServerLog.Clear(); break;

                    /* :start >> Start the server
                     */
                    case ":start": IoC.Container.Get<ServerViewModel>().StartServer(); break;

                    /* :stop >> Stop the server
                     */
                    case ":stop": IoC.Container.Get<ServerViewModel>().Shutdown(); break;

                    /* :help >> Provide help information
                     */
                    case ":help":
                        foreach (string ch in File.ReadAllLines(Environment.CurrentDirectory + "\\Program Files\\Commands.txt"))
                            ServerLog.Add(new LogItem()
                            {
                                Content = ch,
                                ShowTime = false
                            }); break;

                    // Invalid command
                    default:
                        ServerLog.Add(new LogItem() { Content = $"'{CommandString}' is not recognized as a command, use ':h' for help", ShowTime = false });
                        return;
                }
            }
        }

        /// <summary>
        /// When a message is sent from the server, print it to the terminal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServerInformation(object sender, InformationEventArgs e)
        {
            ServerLog.Add(new LogItem()
            {
                Content   = e.Information,
                ShowTime  = (e.InformationTimeStamp != null) ? true : false,
                EventTime = e.InformationTimeStamp,
                Type      = e.MessageType
            }) ;
        }

        /// <summary>
        /// When data is received from a client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataReceived(object sender, ClientInformationEventArgs e)
        {
            // Check what data was sent and from wich client
            if (e.Data.StartsWith("[PRINT]"))
            {
                ServerLog.Add(new LogItem()
                {
                    Content = $"{e.Client.ClientConnection.RemoteEndPoint.ToString()} said: {e.Data.Substring(7)}",
                    EventTime = e.InformationTimeStamp,
                    ShowTime = true,
                    Type = InformationTypes.Information
                });
            }
        }

        /// <summary>
        /// Actions for the commands
        /// </summary>
        #region Command actions

        /// <summary>
        /// Fires when the Enter key has been pressed in the terminal
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

        /// <summary>
        /// Fires when an item has been double clicked on in the explorer
        /// </summary>
        /// <param name="o"></param>
        private void NavigateEvent(object o)
        {
            var Item = o as FileExplorerObject;
        }

        #endregion

        #endregion

    }
}
