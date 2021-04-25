namespace AMCClient2
{
    // Required namespaces
    using System;
    using System.Linq;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using NetworkModules.Core;
    using NetworkModules.Client;
    using System.Windows.Input;
    using AMCCore;

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

        /// <summary>
        /// This is the serverlog that will be displayed in the server console
        /// </summary>
        public ThreadSafeObservableCollection<ILogMessage> ClientLog { get; set; }

        /// <summary>
        /// These are the items that will be displayed in the explorer
        /// </summary>
        public ThreadSafeObservableCollection<FileExplorerObject> ExplorerItems { get; set; }

        /// <summary>
        /// The current path in the explorer
        /// </summary>
        public string CurrentPathOnServerPC { get; set; }

        /// <summary>
        /// The string that is linked to the command box
        /// </summary>
        public string CommandString
        {
            get;
            set;
        } = String.Empty;

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
        public ClientInterfaceViewModel()
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

            // Create the command history
            CommandHistory = new string[0];

            // This is the standard message that shows when the program starts
            ClientLog = new ThreadSafeObservableCollection<ILogMessage>() {
                new LogMessage() { Content = "AMCClient [Version 1.0.0]", ShowTime = false, Type = Responses.Information } ,
                new LogMessage() { Content = "(c) 2021 Stimpon",          ShowTime = false, Type = Responses.Information } ,
            };
            ExplorerItems = new ThreadSafeObservableCollection<FileExplorerObject>();

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand = new RelayCommandNoParameters(ExecuteCommandEvent);
            // Previous command command
            PreviousCommand = new RelayCommandNoParameters(PreviousCommandEvent);
            // Navigate command
            ExplorerDoubleClick = new RelayCommand(NavigateEvent, (o) => { return true; });

            #endregion

            // Subscribe to server events
            Container.Get<ClientHandler>().ClientInformation += OnClientInformation;
            Container.Get<ClientHandler>().DataReceived      += OnDataReceived;
        }

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        private void ResolveCommand()
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
                    case "cls": ClientLog.Clear(); break;

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
                            ClientLog.Add(new LogMessage()
                            {
                                Content = ch,
                                ShowTime = false
                            }); break;

                    // Invalid command
                    default:
                        ClientLog.Add(new LogMessage() { Content = $"'{CommandString}' is not recognized as a command, use ':h' for help", ShowTime = false });
                        return;
                }
            }

            #endregion

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
            // forward the message to the console
            ClientLog.Add(new LogMessage()
            {
                Content = e.Information,
                EventTime = DateTime.Now.ToString(),
                ShowTime = true,
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
                ExplorerItems.Add(new FileExplorerObject()
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
                ExplorerItems.Add(new FileExplorerObject()
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
                ExplorerItems.Add(new FileExplorerObject()
                {
                    Name = data[0],
                    Type = ExplorerItemTypes.Folder,
                    PermissionsDenied = bool.Parse(data[1])
                });
            }

            #endregion

            // If it was no request...
            else
                // Show the data in the terminal
                ClientLog.Add(new LogMessage()
                {
                    Content = Data,
                    EventTime = DateTime.Now.ToString(),
                    ShowTime = true,
                    Type = Responses.Information
                });
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
            
            // Add one slot for the executed command
            Array.Resize(ref CommandHistory, CommandHistory.Length + 1);

            // Add the executed command to the command history
            CommandHistory[CommandHistory.Length - 1] = CommandString;

            // Clear the Input line
            CommandString = String.Empty;
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

        #endregion

        #endregion

    }

}
