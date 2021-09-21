/// <summary>
/// Root namespace
/// </summary>
namespace AMCClient2
{
    #region Required namespaces
    using System;
    using System.Linq;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using NetworkModules.Core;
    using NetworkModules.Client;
    using System.Windows.Input;
    using AMCCore;
    using System.Windows.Data;
    #endregion

    /// <summary>
    /// ViewModel for the Client Interface
    /// </summary>
    public class ClientInterfaceViewModel : BaseInterfaceViewModel
    {
        #region Public Properties

        /// <summary>
        /// Single instance of this ViewModel
        /// </summary>
        public static ClientInterfaceViewModel VM { get; set; }

        /// <summary>
        /// The current path in the explorer
        /// </summary>
        public string CurrentPathOnServerPC { get; set; }

        #endregion

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
        /// When the up key is pressed
        /// </summary>
        public ICommand PreviousCommand { get; set; }

        #endregion

        #region Overrided properties     
        
        /// <summary>
        /// The current position in the <see cref="F:AMCCore.BaseInterfaceViewModel.CommandHistory" />
        /// </summary>
        public override int CurrentCommandIndex { get; set; }
        /// <summary>
        /// The string that is linked to the command box
        /// </summary>
        public override string CommandString { get; set; }

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public override ThreadSafeObservableCollection<ILogMessage> Terminal { get; set; }

        /// <summary>
        /// These are the items that will be displayed in the explorer
        /// </summary>
        public override ThreadSafeObservableCollection<FileExplorerObject> ExplorerItems { get; set; }

        /// <summary>
        /// Contains all the downloads
        /// </summary>
        public override ThreadSafeObservableCollection<IDownloadItem> Downloads { get; set; }

        /// <summary>
        /// Gets or sets the current menu.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public override Menus CurrentMenu { get; set; }

        #endregion

        #region Default constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientInterfaceViewModel() : base()
        {
            if (ProgramState.IsRunning)
                // Call the Init method when a new instance of this VM is created
                Initialize();
        }

        #endregion

        #region private functions

        // One-time functions to run when initializing ======================================================>

        /// <summary>
        /// Creates the commands for the interface.
        /// </summary>
        private void CreateCommands()
        {
            // Command for exetuting commands
            ExecuteCommand = new RelayCommandNoParameters(ExecuteCommandEvent);
            // Previous command command
            PreviousCommand = new RelayCommandNoParameters(PreviousCommandEvent);
            // Navigate command
            ExplorerDoubleClick = new RelayCommand(NavigateEvent, (o) => { return true; });
            // Download command
            DownloadFileClick = new RelayCommand(DownloadEvent);
        }
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to client events
            Container.Get<ClientHandler>().ClientInformation += OnClientInformation;
            Container.Get<ClientHandler>().DataReceived += OnDataReceived;
        }
        /// <summary>
        /// Creates the binding operations.
        /// </summary>
        private void CreateBindingOperations()
        {
            BindingOperations.EnableCollectionSynchronization(Terminal, _TerminalLock);
            BindingOperations.EnableCollectionSynchronization(ExplorerItems, _ExplorerLock);
        }

        // ==================================================================================================>

        /// <summary>
        /// First function that will be called when a new instance
        /// of this ViewModel is created
        /// </summary>
        private void Initialize()
        {
            // Set the accessable ViewModel to this class
            VM = this;

            // This is the standard message that shows when the program starts
            Terminal = new ThreadSafeObservableCollection<ILogMessage>() {
                new LogMessage() { Content = "AMCClient [Version 1.0.0]", ShowTime = false, Type = Responses.Information } ,
                new LogMessage() { Content = "(c) 2021 Stimpon",          ShowTime = false, Type = Responses.Information } ,
            };

            // Create binding operations
            CreateBindingOperations();
            // Subscribe to events
            SubscribeToEvents();
            // Create all the commands
            CreateCommands();
        }


        /// <summary>
        /// Send all servers to the servers
        /// </summary>
        private void SendDrivesToServer()
        {
            // Get all fixed drives from the PC
            var Drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);

            // Loop through all drives
            foreach(var drive in Drives)
            {
                Container.Get<ClientHandler>().Send($"[DRIVE]{drive.Name}|{drive.VolumeLabel}");
            }
        }

        /// <summary>
        /// Navigate to a path
        /// </summary>
        /// <param name="path"></param>
        private void SendPathItems(string path)
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
                            if((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                                if (rule.AccessControlType == AccessControlType.Allow)
                                    PermissionsDenied = false;
                }
                catch { }

                Container.Get<ClientHandler>().Send($"[FOLDER]" +
                    $"{Path.GetFileName(Folder)}|{PermissionsDenied}");
            }
            // Loop through all files and send them to the server
            foreach (var File in Files)
            {
                FileInfo f = new FileInfo(File);
                Container.Get<ClientHandler>().Send($"[FILE]" +
                    $"{f.Name}|{f.Extension}|{f.Length}");
            }
        }

        /// <summary>
        /// Server want a file
        /// </summary>
        /// <param name="FilePath"></param>
        private void SendFileRequest(string FilePath)
        {
            // Check if file exists
            if (!File.Exists(FilePath)) return;

            // Get infor about the file
            FileInfo req_file = new FileInfo(FilePath);

            // SBegin sending the file
            Container.Get<ClientHandler>().BeginSendFile(FilePath);
        }

        /// <summary>
        /// When the <see cref="ClientHandler"/> raises a message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientInformation(object sender, InformationEventArgs e)
        {
            // Declare message string
            string messageString = string.Empty;

            // If this is just a server information
            if (e.Message.GetType().Equals(typeof(ClientMessage)))
            {
                // We only want the message
                messageString = e.Message.Message;
            }
            // Else...
            else
            {
                // Extract code and everything
                messageString = $"{e.Message.Code} - {e.Message.Title}\n{e.Message.Message}";
            }

            // Log the message
            Terminal.Add(new LogMessage()
            {
                Content = messageString,
                ShowTime = (e.InformationTimeStamp != null) ? true : false,
                EventTime = e.InformationTimeStamp,
                Type = e.MessageType
            });
        }

        /// <summary>
        /// When data was received from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Data"></param>
        private void OnDataReceived(object sender, string Data)
        {
            #region When the server navigates this PC

            // If the server wants drive info...
            if (Data.StartsWith("[DRIVES]"))
                SendDrivesToServer();
            // If the server wants to navigate...
            else if (Data.StartsWith("[NAV]"))
                SendPathItems(Data.Substring(5));
            // If the server wants to download a file...
            else if (Data.StartsWith("[DOWNLOAD]"))
                SendFileRequest(Data.Substring(10));

            #endregion

            #region When you navigates the server's PC

            // Client sent a HDD object
            else if (Data.StartsWith("[DRIVE]"))
            {
                // Split the received data
                string[] data = Data.Substring(7).Split('|');
                // Create the object and add it to the list
                AddExplorerItem(new FileExplorerObject()
                {
                    Name = $"{data[1]} ({data[0]})",
                    Path = data[0],
                    Type = ExplorerItemTypes.HDD,
                    PermissionsDenied = false
                });
            }
            // Client sent a file object
            else if (Data.StartsWith("[FILE]"))
            {
                // Split the received data
                string[] data = Data.Substring(6).Split('|');

                // Create the object and add it to the list
                AddExplorerItem(new FileExplorerObject()
                {
                    Name = data[0],
                    Extension = data[1],
                    Size = long.Parse(data[2]),
                    Type = ExplorerItemTypes.File
                });
            }
            // Client sent a folder
            else if (Data.StartsWith("[FOLDER]"))
            {
                // Split the recieved data
                string[] data = Data.Substring(8).Split('|');

                // Create the object and add it to the list
                AddExplorerItem(new FileExplorerObject()
                {
                    Name = data[0],
                    Type = ExplorerItemTypes.Folder,
                    PermissionsDenied = bool.Parse(data[1])
                });
            }

            #endregion

            // If it was no request...
            else
                lock (_TerminalLock)
                {
                    // Show the data in the terminal
                    Terminal.Add(new LogMessage()
                    {
                        Content = Data,
                        EventTime = DateTime.Now.ToString(),
                        ShowTime = true,
                        Type = Responses.Information
                    });
                }
        }

        #endregion

        #region Overrided functions

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        protected override void ResolveCommand()
        {
            // Check if input commands

            // :msg + [STRING] >> Send message to the server
            if (CommandString.ToLower().StartsWith(":msg "))
                Container.Get<ClientHandler>().Send("[PRINT]" + CommandString.Substring(5));

            #region Commands without flags

            else
            {
                switch (CommandString.ToLower())
                {
                    // :connect >> Connect to the server
                    case "connect": Container.Get<ClientHandler>().Connect(); break;

                    // :start >> Start the server
                    case "cls": Terminal.Clear(); break;

                    // :getdrives >> Query drive info from the bound client
                    case "getdrives":
                        // Prepare the explorer
                        ExplorerItems.Clear();
                        // Clear current path
                        CurrentPathOnServerPC = String.Empty;
                        // If the message was sent successfuly, clear the explorer to prepare it for the incoming data
                        Container.Get<ClientHandler>().Send("[DRIVES]");
                        break;

                    // :help >> Provide help information
                    case "help":
                        foreach (string ch in File.ReadAllLines(Environment.CurrentDirectory + "\\Program Files\\Commands.txt"))
                            Terminal.Add(new LogMessage()
                            {
                                Content = ch,
                                ShowTime = false
                            }); break;

                    // Invalid command
                    default:
                        Terminal.Add(new LogMessage() { Content = $"'{CommandString}' is not recognized as a command, use ':h' for help", ShowTime = false });
                        return;
                }
            }

            #endregion

        }

        #endregion

        #region Command actions

        /// <summary>
        /// Is called when an item has been double clicked on in the explorer
        /// </summary>
        /// <param name="o"></param>
        private void NavigateEvent(object o)
        {
            var Item = o as FileExplorerObject;

            // Check if the item is navigateable
            if (Item.Type == ExplorerItemTypes.HDD ||
                Item.Type == ExplorerItemTypes.Folder)
            {

                // Return if server is not granted permissions to that folder or drive
                if (Item.PermissionsDenied)
                    return;

                // Prepare the explorer for new items
                ExplorerItems.Clear();

                // Request navigation
                Container.Get<ClientHandler>().Send((Item.Type == ExplorerItemTypes.Folder) ?
                    $"[NAV]{CurrentPathOnServerPC}\\{Item.Name}" :
                    $"[NAV]{Item.Path}");

                // Set the new path (Formats the string so it looks good)
                CurrentPathOnServerPC += (Item.Type == ExplorerItemTypes.HDD) ? Item.Path.Substring(0, Item.Path.Length - 1) : $"/{Item.Name}";

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

            // Begin receiving the file from the server
            Container.Get<ClientHandler>().ReceiveFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            // Send downloading request to the server
            Container.Get<ClientHandler>().Send($"[DOWNLOAD]{CurrentPathOnServerPC}\\{Item.Name}");
        }

        #endregion

    }

}
