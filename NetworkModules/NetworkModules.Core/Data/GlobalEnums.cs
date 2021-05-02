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
        RW = 2
    }
}
