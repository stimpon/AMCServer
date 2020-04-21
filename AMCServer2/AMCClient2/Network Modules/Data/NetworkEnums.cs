namespace AMCClient2
{
    /// <summary>
    /// Enum for the information event args
    /// </summary>
    public enum Responses
    {
        /// <summary>
        /// Information response
        /// </summary>
        Information,

        /// <summary>
        /// Error response
        /// </summary>
        Error,
        /// <summary>
        /// Warning response
        /// </summary>
        Warning,
        /// <summary>
        /// OK response
        /// </summary>
        OK
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
}
