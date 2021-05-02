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
}
