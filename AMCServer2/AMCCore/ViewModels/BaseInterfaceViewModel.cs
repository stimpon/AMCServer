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

        #endregion

        #region Current download properties

        /// <summary>
        /// Name of the file
        /// </summary>
        public abstract string FileName { get; set; }
        /// <summary>
        /// Size of the file
        /// </summary>
        public abstract decimal Size { get; set; }
        /// <summary>
        /// Downloaded bytes
        /// </summary>
        public abstract decimal ActualSize { get; set; }
        /// <summary>
        /// String for the View
        /// </summary>
        public abstract string ProgresString { get; set; }

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
            Size = 1;
            ActualSize = 0;
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
        /// When new information about a download is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDownloadInformation(object sender, FileDownloadInformationEventArgs e)
        {
            FileName = e.FileName;
            Size = e.FileSize;
            ActualSize = e.ActualFileSize;

            string SizeString = StringFormatingHelpers.BytesToSizeString(Size);
            string ActualSizeString = StringFormatingHelpers.BytesToSizeString(ActualSize);

            ProgresString = $"{ActualSizeString}/{SizeString}";
        }

        /// <summary>
        /// Adds the explorer item.
        /// </summary>
        /// <param name="object">The object.</param>
        protected void AddExplorerItem(FileExplorerObject _object)
        {
            // UI cross thread solution
            lock (_ExplorerLock)
            {
                // Add the object to the file explorer
                ExplorerItems.Add(_object);
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
