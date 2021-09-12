namespace NetworkModules.Core
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
    /// Depending on the application, this functionality may not be needed.
    /// </summary>
    public enum Permissions
    {
        /// <summary>
        /// No permissions
        /// </summary>
        N = 0,

        /// <summary>
        /// Read permissions
        /// </summary>
        R = 1,

        /// <summary>
        /// Read and write permissions
        /// </summary>
        RW = 2,

    }

    /// <summary>
    /// Modes stating what to do with a file
    /// </summary>
    public enum FileModes
    {
        /// <summary>
        /// Nothing is expected to connect to the FTP socket
        /// </summary>
        None = -1,

        /// <summary>
        /// Server => Client
        /// </summary>
        Send = 0,

        /// <summary>
        /// Server <= Client
        /// </summary>
        Download = 1
    }
}
