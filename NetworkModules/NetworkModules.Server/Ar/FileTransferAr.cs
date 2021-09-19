/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Server
{
    // Rrequred namespaces
    using NetworkModules.Core;
    using System.Net.Sockets;

    public class FileTransferAr
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public FileModes Mode { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public ConnectionViewModel Client { get; set; }

        /// <summary>
        /// Gets or sets the optional parameters.
        /// </summary>
        public object OptionalParameters { get; set; }
    }
}
