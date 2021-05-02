namespace AMCServer2
{
    // Required namespaces
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Windows.Input;
    using NetworkModules.Core;
    using NetworkModules.Server;
    using AMCCore;
    using System.Windows;

    /// <summary>
    /// ViewModel for the ServerView
    /// </summary>
    public class ServerInterfaceViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static ServerInterfaceViewModel VM { get; set; }

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
        public string CommandString { get; set; } = String.Empty;

        #region Commands

        /// <summary>
        /// When a command is exeuted in the terminal
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// When an item is double clicked on in the explorer
        /// </summary>
        public ICommand ExplorerDoubleClick { get; set; }

        /// <summary>
        /// When the download button is clicked on in the explorer
        /// </summary>
        public ICommand DownloadFileClick { get; set; }

        /// <summary>
        /// When to show the previous command
        /// </summary>
        public ICommand PreviousCommand { get; set; }

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

        #endregion

        #region Private members

        /// <summary>
        /// The current position in the <see cref="CommandHistory"/>
        /// </summary>
        private int CurrentCommandIndex { get; set; } = 0;

        /// <summary>
        /// Array to save all executed commands
        /// </summary>
        private string[] CommandHistory;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServerInterfaceViewModel()
        {
            if (ProgramState.IsRunning)
                // Call the Init method when a new instance of this VM is created
                Initialize();

        }

        #region Functions

        /// <summary>
        /// Adds the file explorer item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AddFileExplorerItem(FileExplorerObject item)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Add item to the control
                ExplorerItems.Add(item);
            }));
        }

        /// <summary>
        /// First function that will be called when a new instance
        /// of this ViewModel is created
        /// </summary>
        private void Initialize()
        {
            // Set the accessable ViewModel to this class
            VM = this;

            // Initialize a new command history
            CommandHistory = new string[0];

            // This is the standard message that shows when the program starts
            ServerLog = new ThreadSafeObservableCollection<ILogMessage>() {
                new LogMessage() { Content = "AMCServer [Version 1.0.0]", ShowTime = false, Type = Responses.Information } ,
                new LogMessage() { Content = "(c) 2021 Stimpon",          ShowTime = false, Type = Responses.Information } ,
            };
            ExplorerItems = new ThreadSafeObservableCollection<FileExplorerObject>();

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand      = new RelayCommandNoParameters(ExecuteCommandEvent);
            // Navigate command
            ExplorerDoubleClick = new RelayCommand(NavigateEvent, (o) => { return true; });
            // Download command
            DownloadFileClick   = new RelayCommand(DownloadEvent);
            // Previous command command
            PreviousCommand = new RelayCommandNoParameters(PreviousCommandEvent);

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
            #region Commands with flags

            // :bind + [ID] >> Bind the server to a client
            if (CommandString.ToLower().StartsWith("bind "))
            {
                // Try to parse the argument into an int
                if (int.TryParse(CommandString.Substring(5), out int ID))
                    Container.GetSingleton<ServerHandler>().BindServer(ID);
                else
                    ServerLog.Add(new LogMessage() { Content  = $"'{CommandString[5..]}' is an invalid ID", 
                                                  ShowTime = false, 
                                                  Type     = Responses.Error });
            }
            // :setpriv [ID] [PL] >> Set privileges for an active connection
            else if (CommandString.ToLower().StartsWith("setpriv "))
            {
                // Extract substrings from command
                var commandFlags = CommandString.Split(' ')[1..];

                // If a corrent client id was specified and a corrent privilege digit was specified
                if (commandFlags.Length.Equals(2) && int.TryParse(commandFlags[0], out int id))
                {
                    bool requestFulfilled = false, privilegesLoaded = false;

                    // Load the provided privilege from the privilege set
                    privilegesLoaded = Enum.TryParse(typeof(Permissions), commandFlags[1], true, out object newPrivileges);

                    // If the privilege is in the priviilege-set
                    if(privilegesLoaded)
                        // tell server to change privileges for the specified client if valid privileges was provided
                        requestFulfilled = Container.GetSingleton<ServerHandler>().SetClientPrivileges(id, (Permissions)newPrivileges);

                    // If the command executed successful
                    if (requestFulfilled)
                    {
                        // Tell that client that he now has new privileges
                        Container.GetSingleton<ServerHandler>().Send($"New privileges was set for you by the server- { commandFlags[1].ToUpper() }", id);

                        // Display message
                        ServerLog.Add(new LogMessage() { Content = $"New privileges was set for client {id}- { commandFlags[1].ToUpper() }", Type = Responses.OK });
                    }
                    // If privileges could not be set
                    else
                        // Display error message
                        ServerLog.Add(new LogMessage() { Content = $"Client {id} was not found, or a bad privileges was provided", Type = Responses.Error });
                }
                // Else if the command had invalid or to few flags...
                else
                    // Display error message
                    ServerLog.Add(new LogMessage() { Content = $"Invalid or to few command flags, check :help", Type = Responses.Information });
            }
            // start + parameters for what services to start
            else if (CommandString.ToLower().StartsWith("start"))
            {
                // Get all flags
                var flags = CommandString.Split(' ')[1..];

                // If no flags was provided
                if (flags.Count() == 0)
                {
                    ServerLog.Add(new LogMessage() { Content = "You must say which services to start", Type = Responses.Information });
                }
                // Else if any flags was provided...
                else
                {
                    // Check at the end if this has been set to true
                    bool validFlags = false;

                    // If a 'start all services' flag is in the command
                    if (flags.Contains("-a"))
                    {
                        // Start the server
                        Container.GetSingleton<ServerHandler>().StartServer();
                        // start the file transfer socket
                        Container.GetSingleton<ServerHandler>().StartFileTransferSocket();

                        // Valid flags was provided
                        validFlags = true;
                    }
                    // Else... Check wcich services to start
                    else
                    {
                        // Start the server...
                        if (flags.Contains("-s"))
                        {
                            // Start the server
                            Container.GetSingleton<ServerHandler>().StartServer();
                            // Valid flags was provided
                            validFlags = true;
                        }
                        // Start the file transfer socket
                        if (flags.Contains("-f"))
                        {
                            // Start the server
                            Container.GetSingleton<ServerHandler>().StartFileTransferSocket();
                            // Valid flags was provided
                            validFlags = true;
                        }
                    }

                    // If no valid flags was provided
                    if (!validFlags)
                    {
                        ServerLog.Add(new LogMessage() { Content = "Invalid parameters was provided, type 'help' for more information", Type = Responses.Information });
                    }
                }
            }
            // stop + parameters for what services to stop
            else if (CommandString.ToLower().StartsWith("stop"))
            {
                // Get all flags
                var flags = CommandString.Split(' ')[1..];

                // If no flags was provided
                if (flags.Count() == 0)
                {
                    // Stop all services
                    Container.GetSingleton<ServerHandler>().StopServer();
                }
                // Else if any flags was provided...
                else
                {
                    // Check at the end if this has been set to true
                    bool validFlags = false;

                    // If the file-transfer socket should be started
                    if (flags.Contains("-f"))
                    {
                        // Start the file-transfer socket
                        Container.GetSingleton<ServerHandler>().StopFileTransferSocket();

                        // Valid flags was provided
                        validFlags = true;
                    }

                    // If no valid flags was provided
                    if (!validFlags)
                    {
                        ServerLog.Add(new LogMessage() { Content = "Invalid parameters was provided, type 'help' for more information", Type = Responses.Information });
                    }
                }
            }
            #endregion

            #region Commands without flags

            else
            {
                // Check the command (NOT CASE SENSITIVE)
                switch (CommandString.ToLower())
                {
                    // :cls >> Clear the console                   
                    case "cls": ServerLog.Clear(); break;

                    // :unbind >> unbind the server from the currently bound client
                    case "unbind": Container.GetSingleton<ServerHandler>().UnbindServer(); break;

                    // :getdrives >> Query drive info from the bound client
                    case "getdrives":
                        // Prepare the explorer
                        ExplorerItems.Clear();
                        // Clear current path
                        CurrentPathOnClientPC = String.Empty;
                        // If the message was sent successfuly, clear the explorer to prepare it for the incoming data
                        Container.GetSingleton<ServerHandler>().Send("[DRIVES]");
                        break;

                    // :help >> Provide help information
                    case "help":
                        // Read all lines from the help file
                        foreach (string ch in File.ReadAllLines(Environment.CurrentDirectory + "\\Program Files\\Commands.txt"))
                            // Display each line correctly in the terminal
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
            #endregion
        }

        /// <summary>
        /// When the <see cref="ServerHandler"/> raises a message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServerInformation(object sender, InformationEventArgs e)
        {
            // Log the message
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

            #region When you navigates a client's PC

            // Client sent a HDD object
            if (e.Data.StartsWith("[DRIVE]"))
            {
                // Split the received data
                string[] data = e.Data.Substring(7).Split('|');

                AddFileExplorerItem(new FileExplorerObject()
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

                // Add item to the file explorer
                AddFileExplorerItem(new FileExplorerObject()
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

                // Add the file explorer item
                AddFileExplorerItem(new FileExplorerObject()
                {
                    Name = data[0],
                    Type = ExplorerItemTypes.Folder,
                    PermissionsDenied = bool.Parse(data[1])
                });
            }

            #endregion

            #region When the client navigates this PC

            // Client must have atleast read permissions to navigate the server PC
            else if (e.Client.ServerPermissions > 0)
            {
                // If the server wants drive info...
                if (e.Data.StartsWith("[DRIVES]")) SendDrivesToClient(e.Client.ID);
                // If the server wants to navigate...
                else if (e.Data.StartsWith("[NAV]")) SendPathItems(e.Data.Substring(5), e.Client.ID);
            }

            // Else if the client does not have the required permissions
            else
                Container.GetSingleton<ServerHandler>().Send($"Request was denied", e.Client.ID);

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

            // Add one slot to the command history
            Array.Resize(ref CommandHistory, CommandHistory.Length + 1);

            // Add the current command to the history after execution
            CommandHistory[CommandHistory.Length - 1] = CommandString;

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

        /// <summary>
        /// Is called when the up key is pressed
        /// </summary>
        private void PreviousCommandEvent()
        {
            // Return if there are no commands in the command history
            if (CommandHistory.Length < 1) return;

            // Reset the CommandIndex if it is at the end of the history
            if (CurrentCommandIndex < 0) CurrentCommandIndex = CommandHistory.Length - 1;

            // Set the current command to the previous one in the history
            CommandString = CommandHistory[CurrentCommandIndex];

            // Go back one step in the history
            CurrentCommandIndex--;
        }


        /// <summary>
        /// Send all servers to the servers
        /// </summary>
        private void SendDrivesToClient(int ClientID)
        {
            // Get all fixed drives from the PC
            var Drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);

            // Loop through all drives
            foreach (var drive in Drives)
            {
                Container.GetSingleton<ServerHandler>().Send($"[DRIVE]{drive.Name}|{drive.VolumeLabel}", ClientID);
            }
        }

        /// <summary>
        /// Navigate to a path
        /// </summary>
        /// <param name="path"></param>
        private void SendPathItems(string path, int ClientID)
        {
            // Read all directories from the requested path
            var Folders = Directory.GetDirectories(path);
            // Read all files from the requested path
            var Files = Directory.GetFiles(path);

            // Loop through all folders and send them to the server
            foreach (var Folder in Folders)
            {
                // Tells the server if read permissions is allowed
                bool PermissionsDenied = true;

                try
                {
                    // Test if access is denied
                    Directory.GetFiles(Folder);

                    // Get the access rules for that folder
                    var rules = new FileInfo(Folder).GetAccessControl()
                                                    .GetAccessRules(true,
                                                                    true,
                                                                    typeof(SecurityIdentifier));
                    // Check if read permissions are allowed
                    if (rules != null)
                        foreach (FileSystemAccessRule rule in rules)
                            if ((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                                if (rule.AccessControlType == AccessControlType.Allow)
                                    PermissionsDenied = false;
                }
                catch { }

                Container.GetSingleton<ServerHandler>().Send($"[FOLDER]" +
                    $"{Path.GetFileName(Folder)}|{PermissionsDenied}", ClientID);
            }
            // Loop through all files and send them to the server
            foreach (var File in Files)
            {
                FileInfo f = new FileInfo(File);
                Container.GetSingleton<ServerHandler>().Send($"[FILE]" +
                    $"{f.Name}|{f.Extension}|{f.Length}", ClientID);
            }
        }

        #endregion

        #endregion

    }
}
