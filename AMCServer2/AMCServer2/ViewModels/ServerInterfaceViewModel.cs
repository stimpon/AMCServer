namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.IO;
    using NetworkModules.Core;
    using NetworkModulesServer;
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

        /// <summary>
        /// When an item is double clicked on in the explorer
        /// </summary>
        public RelayCommand ExplorerDoubleClick { get; set; }

        /// <summary>
        /// When the download button is clicked on in the explorer
        /// </summary>
        public RelayCommand DownloadFileClick { get; set; }

        #endregion

        #region Current download properties

        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Size of the file
        /// </summary>
        public decimal Size { get; set; } = 1;
        /// <summary>
        /// Downloaded bytes
        /// </summary>
        public decimal ActualSize { get; set; } = 0;
        /// <summary>
        /// String for the View
        /// </summary>
        public string ProgresString { get; set; }

        #endregion

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public ThreadSafeObservableCollection<ILogMessage> ServerLog { get; set; }

        /// <summary>
        /// These are the items that will be displayed in the explorer
        /// </summary>
        public ThreadSafeObservableCollection<FileExplorerObject> ExplorerItems { get; set; }

        /// <summary>
        /// The current path in the explorer
        /// </summary>
        public string CurrentPathOnClientPC { get; set; }

        /// <summary>
        /// The string that is linked to the command box
        /// </summary>
        public string CommandString  { get;  set; } = String.Empty;

        #endregion

        /// <summary>
        /// Default constructor
        public ServerInterfaceViewModel()
        /// </summary>
        {
            if (ProgramState.IsRunning)
                // Call the Init method when a new instance of this VM is created
                Initialize();
        }

        #region Functions

        /// <summary>
        /// First function that will be called when a new instance
        /// of this ViewModel is created
        /// </summary>
        private void Initialize()
        {
            // Set the accessable ViewModel to this class
            VM = this;

            // This is the standard message that shows when the program starts
            ServerLog = new ThreadSafeObservableCollection<ILogMessage>() {
                new LogMessage() { Content = "AMCServer [Version 1.0.0]", ShowTime = false, Type = Responses.Information } ,
                new LogMessage() { Content = "(c) 2020 Stimpon",          ShowTime = false, Type = Responses.Information } ,
            };
            ExplorerItems = new ThreadSafeObservableCollection<FileExplorerObject>();

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand      = new RelayCommandNoParameters(ExecuteCommandEvent);
            // Navigate command
            ExplorerDoubleClick = new RelayCommand(NavigateEvent, (o) => { return true; });
            // Download command
            DownloadFileClick   = new RelayCommand(DownloadEvent);

            #endregion

            // Subscribe to server events
            Container.GetSingleton<ServerHandler>().NewServerInformation += OnServerInformation;
            Container.GetSingleton<ServerHandler>().NewDataReceived      += OnDataReceived;
            Container.GetSingleton<ServerHandler>().DownloadInformation  += OnDownloadInformation;

        }

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        private void ResolveCommand()
        {
            // Check if input command

            // :bind + [ID] >> Bind the server to a client
            if(CommandString.ToLower().StartsWith(":bind "))
            {
                // Try to parse the argument into an int
                if (int.TryParse(CommandString.Substring(5), out int ID))
                    Container.GetSingleton<ServerHandler>().BindServer(ID);
                else
                    ServerLog.Add(new LogMessage() { Content  = $"'{CommandString.Substring(5)}' is an invalid ID", 
                                                  ShowTime = false, 
                                                  Type     = Responses.Error });
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
                    case ":start": Container.GetSingleton<ServerHandler>().StartServer(); break;

                    /* :stop >> Stop the server
                     */
                    case ":stop": Container.GetSingleton<ServerHandler>().StopServer(); break;

                    case ":unbind": Container.GetSingleton<ServerHandler>().UnbindServer(); break;

                    case ":getdrives":
                        // Prepare the explorer
                        ExplorerItems.Clear();
                        // Clear current path
                        CurrentPathOnClientPC = String.Empty;
                        // If the message was sent successfuly, clear the explorer to prepare it for the incoming data
                        Container.GetSingleton<ServerHandler>().Send("[DRIVES]");
                        break;

                    /* :help >> Provide help information
                     */
                    case ":help":
                        foreach (string ch in File.ReadAllLines(Environment.CurrentDirectory + "\\Program Files\\Commands.txt"))
                            ServerLog.Add(new LogMessage()
                            {
                                Content = ch,
                                ShowTime = false
                            }); break;

                    // Invalid command
                    default:
                        ServerLog.Add(new LogMessage() { Content = $"'{CommandString}' is not recognized as a command, use ':h' for help", ShowTime = false });
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
            ServerLog.Add(new LogMessage()
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
                // Add the message to the server log
                ServerLog.Add(new LogMessage() { Content = $"{e.Client.ClientConnection.RemoteEndPoint.ToString()} said: {e.Data.Substring(7)}",
                                              EventTime = e.InformationTimeStamp,
                                              ShowTime = true,
                                              Type = Responses.Information });
            }

            #region Navigation

            // Client sent a HDD object
            if (e.Data.StartsWith("[DRIVE]"))
            {
                // Split the received data
                string[] data = e.Data.Substring(7).Split('|');
                // Create the object and add it to the list
                ExplorerItems.Add(new FileExplorerObject()
                {
                    Name = $"{data[1]} ({data[0]})",
                    Path = data[0],
                    Type = ExplorerItemTypes.HDD,
                    PermissionsDenied = false
                });
            }
            // Client sent a file object
            else if (e.Data.StartsWith("[FILE]"))
            {
                // Split the received data
                string[] data = e.Data.Substring(6).Split('|');
                // Create the object and add it to the list
                ExplorerItems.Add(new FileExplorerObject()
                {
                    Name = data[0],
                    Extension = data[1],
                    Size = long.Parse(data[2]),
                    Type = ExplorerItemTypes.File
                });
            }
            // Client sent a folder
            else if (e.Data.StartsWith("[FOLDER]"))
            {
                // Split the recieved data
                string[] data = e.Data.Substring(8).Split('|');

                // Create the object and add it to the list
                ExplorerItems.Add(new FileExplorerObject()
                {
                    Name = data[0],
                    Type = ExplorerItemTypes.Folder,
                    PermissionsDenied = bool.Parse(data[1])
                });
            }

            #endregion
        }

        /// <summary>
        /// When new information about a download is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadInformation(object sender, FileDownloadInformationEventArgs e)
        {
            FileName   = e.FileName;
            Size       = e.FileSize;
            ActualSize = e.ActualFileSize;

            string SizeString       = StringFormatingHelpers.BytesToSizeString(Size);
            string ActualSizeString = StringFormatingHelpers.BytesToSizeString(ActualSize);

            ProgresString = $"{ActualSizeString}/{SizeString}";
        }

        /// <summary>
        /// Actions for the commands
        /// </summary>
        #region Command actions

        /// <summary>
        /// Is called when the Enter key has been pressed in the terminal
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
        /// Is called when an item has been double clicked on in the explorer
        /// </summary>
        /// <param name="o"></param>
        private void NavigateEvent(object o)
        {
            var Item = o as FileExplorerObject;

            // Check if the item is navigateable
            if( Item.Type == ExplorerItemTypes.HDD || 
                Item.Type == ExplorerItemTypes.Folder) {

                // Return if server is not granted permissions to that folder or drive
                if (Item.PermissionsDenied)
                    return;

                // Prepare the explorer for new items
                ExplorerItems.Clear();

                // Request navigation
                Container.GetSingleton<ServerHandler>().Send((Item.Type == ExplorerItemTypes.Folder) ? 
                    $"[NAV]{CurrentPathOnClientPC}\\{Item.Name}" : 
                    $"[NAV]{Item.Path}");

                // Set the new path (Formats the string so it looks good)
                CurrentPathOnClientPC += (Item.Type == ExplorerItemTypes.HDD) ? Item.Path.Substring(0, Item.Path.Length - 1) : $"/{Item.Name}"; 
                                                                                 
            }
        }

        /// <summary>
        /// Is called when the download button is clicked on a file
        /// </summary>
        /// <param name="o"></param>
        private void DownloadEvent(object o)
        {
            // Get the clicked file
            var Item = o as FileExplorerObject;

            // Begin receiving the file
            Container.GetSingleton<ServerHandler>().ReceiveFile(Container.GetSingleton<ServerHandler>().BoundClient,
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            // Send downloading request
            Container.GetSingleton<ServerHandler>().Send($"[DOWNLOAD]{CurrentPathOnClientPC}\\{Item.Name}");
        }

        #endregion

        #endregion

    }
}
