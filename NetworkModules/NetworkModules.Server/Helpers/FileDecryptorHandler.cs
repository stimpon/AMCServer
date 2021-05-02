/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Server
{
    #region Namespaces
    using NetworkModules.Core;
    using System.Security.Cryptography;
    #endregion

    /// <summary>
    /// Handles file decryption (Server side)
    /// </summary>
    public class FileDecryptorHandler : FileDecryptor
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the sender if the file.
        /// </summary>
        public ConnectionViewModel Sender { get; set; }

        #endregion

        /// <summary>
        /// Default constructor, see: <see cref="FileDecryptor"/>
        /// </summary>
        public FileDecryptorHandler(AesCryptoServiceProvider Decryptor, string FileName, string Path, long FileSize)
            // Pass the parameters to the base constructor
            : base(Decryptor, FileName, Path, FileSize) { }
    }
}
