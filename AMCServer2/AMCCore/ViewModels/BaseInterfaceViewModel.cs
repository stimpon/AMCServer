/// <summary>
/// Root namespace
/// </summary>
namespace AMCCore
{
    #region Required namespaces
    using System;
    using NetworkModules.Core;
    #endregion

    /// <summary>
    /// This is a base viewmodel for all interface view models, this is so we don't need to duplicate logic
    /// that is shared between different interfaces
    /// </summary>
    public abstract class BaseInterfaceViewModel : BaseViewModel
    {
        #region protected members

        /// <summary>
        /// Array to save all executed commands
        /// </summary>
        protected string[] CommandHistory;

        /// <summary>
        /// The download path
        /// </summary>
        protected string DownloadPath { get; set; }

        #endregion

        #region UI Locks

        /// <summary>
        /// The log lock
        /// </summary>
        protected object _TerminalLock;

        /// <summary>
        /// The explorer items lock
        /// </summary>
        protected object _ExplorerLock;

        /// <summary>
        /// The downloads lock
        /// </summary>
        protected object _DownloadsLock;

        #endregion

        #region Abtract properties

        /// <summary>
        /// The current position in the <see cref="CommandHistory"/>
        /// </summary>
        public abstract int CurrentCommandIndex { get; set; }

        /// <summary>
        /// The string that is linked to the command box
        /// </summary>
        public abstract string CommandString { get; set; }

        /// <summary>
        /// Gets or sets the explorer items.
        /// </summary>
        public abstract ThreadSafeObservableCollection<FileExplorerObject> ExplorerItems { get; set; }

        /// <summary>
        /// This is the items in the terminal
        /// </summary>
        public abstract ThreadSafeObservableCollection<ILogMessage> Terminal { get; set; }

        /// <summary>
        /// Contains all the downloads
        /// </summary>
        public abstract ThreadSafeObservableCollection<IDownloadItem> Downloads { get; set; }

        /// <summary>
        /// Gets or sets the current menu.
        /// </summary>
        public abstract Menus CurrentMenu { get; set; }

        #endregion

        #region Default constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseInterfaceViewModel"/> class.
        /// </summary>
        public BaseInterfaceViewModel()
        {
            // Set default values and instanciate objects
            _TerminalLock = new object();
            _ExplorerLock = new object();
            _DownloadsLock = new object();
            CommandHistory = new string[0];
            ExplorerItems = new ThreadSafeObservableCollection<FileExplorerObject>();
        }

        #endregion

        #region Protected functions

        /// <summary>
        /// Is called when the Enter key has been pressed in the terminal
        /// </summary>
        protected void ExecuteCommandEvent()
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
            CommandString = string.Empty;

            // reset the current command index
            CurrentCommandIndex = CommandHistory.Length;
        }

        /// <summary>
        /// Is called when the up key is pressed
        /// </summary>
        protected virtual void PreviousCommandEvent()
        {
            // Return if there are no commands in the command history
            if (CommandHistory.Length < 1) return;

            // Go to the previous command index
            CurrentCommandIndex--;

            // Reset the command index if we are at the begining
            if (CurrentCommandIndex < 0) CurrentCommandIndex = CommandHistory.Length - 1;

            // Display the previous command
            CommandString = CommandHistory[CurrentCommandIndex];
        }

        /// <summary>
        /// Is called when the down key is pressed
        /// </summary>
        protected virtual void NextCommandEvent()
        {
            // Return if there are no commands in the command history
            if (CommandHistory.Length < 1) return;

            // Go to the next command
            CurrentCommandIndex++;

            // Reset the command index if we are at the end
            if (CurrentCommandIndex > CommandHistory.Length - 1) CurrentCommandIndex = 0;

            // Display the previous command
            CommandString = CommandHistory[CurrentCommandIndex];
        }

        /// <summary>
        /// Adds the explorer item.
        /// </summary>
        /// <param name="object">The object.</param>
        protected virtual void AddExplorerItem(FileExplorerObject _object)
        {
            // UI cross thread solution
            lock (_ExplorerLock)
            {
                // Add the object to the file explorer
                ExplorerItems.Add(_object);
            }
        }

        /// <summary>
        /// Adds the download item.
        /// </summary>
        /// <param name="download">The download.</param>
        protected virtual void AddDownloadItem(IDownloadItem download)
        {
            // Lock the UI
            lock (_DownloadsLock)
            {
                // Add the item
                Downloads.Add(download);
            }
        }

        #endregion

        #region Abstract functions

        /// <summary>
        /// Resolves the command that is executed.
        /// </summary>
        protected abstract void ResolveCommand();

        #endregion
    }
}
