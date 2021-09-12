namespace NetworkModules.Server
{
    // Namespaces
    using NetworkModules.Core;

    /// <summary>
    /// Async Result object for the file-transfer socket accept callback
    /// </summary>
    public class FTar
    {
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public ConnectionViewModel Client { get; set; }

        /// <summary>
        /// Gets or sets the action to preform on for this connection
        /// </summary>
        public FileModes Action { get; set; }

        /// <summary>
        /// Gets or sets the optional parameters.
        /// </summary>
        public object OptionalParameters { get; set; }
    }
}
