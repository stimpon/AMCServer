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
    /// Ar object for the FT socket when downloading
    /// </summary>
    public partial class FileTransferDownloadAr
    {
        /// <summary>
        /// Gets or sets the decryptor.
        /// </summary>
        /// <value>
        /// The decryptor.
        /// </value>
        public FileDecryptor Decryptor { get; set; }

        /// <summary>
        /// Gets or sets the connection used for this transfer.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public Socket connection { get; set; }
    }
}
