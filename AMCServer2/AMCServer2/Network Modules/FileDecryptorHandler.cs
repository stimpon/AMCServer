namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System.IO;
    using System.Security.Cryptography;
    #endregion

    /// <summary>
    /// Carries information about a download
    /// </summary>
    internal class FileDecryptorHandler
    {
        /// <summary>
        /// Name of the file being downloaded
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The actual size of the file in bytes
        /// </summary>
        public long? ActualFileSize { get => Stream?.Length; }

        /// <summary>
        /// Sender
        /// </summary>
        public ClientViewModel Sender {get; set;}

        /// <summary>
        /// The file stream
        /// </summary>
        public FileStream Stream { get; set; }
        /// <summary>
        /// Cryptographic stream
        /// </summary>
        public CryptoStream CryptoStream { get; set; }
    }
}
