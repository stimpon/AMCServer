namespace NetworkModules.Server
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
    /// All services that the server provides for the clients
    /// </summary>
    public enum ClientServices
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
