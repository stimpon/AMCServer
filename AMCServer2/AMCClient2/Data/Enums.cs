/// <summary>
/// Contains all of the public Enums
/// </summary>
namespace AMCClient2
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
    public enum ClientStates
    {
        /// <summary>
        /// When the client is connected to the server
        /// </summary>
        Connected,

        /// <summary>
        /// While the client is connecting to the server
        /// </summary>
        Connecting,

        /// <summary>
        /// While the client disconnects from the server
        /// </summary>
        Disconnecting,

        /// <summary>
        /// When the client is disconnected from the server
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// These are the main Views for the MainWindow
    /// </summary>
    public enum MainViews
    {
        ServerInterface
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
