/// <summary>
/// Contains all of the public Enums
/// </summary>
namespace AMCServer2
{
    /// <summary>
    /// Enum for the information event args
    /// </summary>
    public enum InformationTypes
    {
        /// <summary>
        /// Display information
        /// </summary>
        Information,

        /// <summary>
        /// returned a warning
        /// </summary>
        Warning,

        /// <summary>
        /// returned an error
        /// </summary>
        Error,

        /// <summary>
        /// Proceeded action was successful
        /// </summary>
        ActionSuccessful,

        /// <summary>
        /// Proceeded action was not successful
        /// </summary>
        ActionFailed
    }

    /// <summary>
    /// All of the different server states
    /// </summary>
    public enum ServerStates
    {
        /// <summary>
        /// The server is not running
        /// </summary>
        Offline,

        /// <summary>
        /// When the server is starting up
        /// </summary>
        StartingUp,

        /// <summary>
        /// When the server is starting up
        /// </summary>
        ShuttingDown,

        /// <summary>
        /// The server is running
        /// </summary>
        Online
    }

    /// <summary>
    /// These are the main Views for the MainWindow
    /// </summary>
    public enum MainViews
    {
        ServerInterface
    }

    /// <summary>
    /// All services that the server provides for the clients
    /// </summary>
    public enum Services
    {
        /// <summary>
        /// serverclient
        /// </summary>
        AMCClient,

        /// <summary>
        /// Cryptochat service
        /// </summary>
        Transparent
    }

    /// <summary>
    /// All item types that can be displayed in the explorer view
    /// </summary>
    public enum ExplorerItemTypes
    {
        HDD,
        File,
        Folder
    }
}
