/// <summary>
/// Contains all of the public Enums
/// </summary>
namespace AMCServer2
{
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
}
