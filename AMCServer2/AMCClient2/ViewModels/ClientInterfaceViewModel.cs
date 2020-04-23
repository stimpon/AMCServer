namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Linq;
    using System.IO;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
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

        public RelayCommand ExplorerDoubleClick { get; set; }

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
            ClientLog = new ThreadSafeObservableCollection<ILogMessage>() {
                new LogMessage() { Content = "AMCClient [Version 1.0.0]", ShowTime = false, Type = Responses.Information } ,
                new LogMessage() { Content = "(c) 2020 Stimpon",          ShowTime = false, Type = Responses.Information } ,
            };

            #region Create commands

            // Command for exetuting commands
            ExecuteCommand = new RelayCommandNoParameters(ExecuteCommandEvent);

            #endregion

            // Subscribe to server events
            IoC.Container.Get<ClientViewModel>().ClientInformation += OnClientInformation;
            IoC.Container.Get<ClientViewModel>().DataReceived      += OnDataReceived;

        }

        /// <summary>
        /// Resoleves the executed command
        /// </summary>
        private void ResolveCommand()
        {
            // Check if input commands

            // :msg + [STRING] >> Send message to the server
            if (CommandString.ToLower().StartsWith(":msg "))
                IoC.Container.Get<ClientViewModel>().Send("[PRINT]" + CommandString.Substring(5));

            // Check if standalone command
            else
            {
                switch (CommandString.ToLower())
                {
                    // :connect >> Connect to the server
                    case ":connect": IoC.Container.Get<ClientViewModel>().Connect(); break;

                    /* :cls >> Clear the console
                     */
                    case ":cls": ClientLog.Clear(); break;

                    /* :help >> Provide help information
                     */
                    case ":help":
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
                IoC.Container.Get<ClientViewModel>().Send($"[DRIVE]{drive.Name}|{drive.VolumeLabel}");
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

                IoC.Container.Get<ClientViewModel>().Send($"[FOLDER]" +
                    $"{Path.GetFileName(Folder)}|{PermissionsDenied}");
            }
            // Loop through all files and send them to the server
            foreach (var File in Files)
            {
                FileInfo f = new FileInfo(File);
                IoC.Container.Get<ClientViewModel>().Send($"[FILE]" +
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
            IoC.Container.Get<ClientViewModel>().BeginSendFile(FilePath);
        }

        /// <summary>
        /// When a message is sent from the server, print it to the terminal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientInformation(object sender, InformationEventArgs e)
        {
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
            if (Data.StartsWith("[DRIVES]"))
                SendDrivesToServer();
            if (Data.StartsWith("[NAV]"))
                SendPathItems(Data.Substring(5));
            if (Data.StartsWith("[DOWNLOAD]"))
                SendFileRequest(Data.Substring(10));
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
