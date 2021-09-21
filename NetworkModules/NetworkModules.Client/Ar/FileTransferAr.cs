/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Client
{
    #region Required namespaces
    using NetworkModules.Core;
    using System.Net.Sockets;
    #endregion

    /// <summary>
    /// Ar object for the FT socket
    /// </summary>
    public partial class FileTransferAr
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public FileModes Mode { get; set; }

        /// <summary>
        /// Gets or sets the connection used for this transfer.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public Socket connection { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional parameters.
        /// </summary>
        public object OptionalParameters { get; set; }
    }
}
