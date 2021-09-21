/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Core
{
    // Required namespaces
    using NetworkModules.Core;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Security.Cryptography;

    /// <summary>
    /// Handler for a download
    /// </summary>
    public class DownloadHandler : FileDecryptor
    {
        /// <summary>
        /// Gets or sets this download's identifier.
        /// </summary>
        public string DownloadID { get; set; }

        /// <summary>
        /// Socket used for downloading this file
        /// </summary>
        public Socket DownloadSocket { get; set; }

        /// <summary>
        /// Gets or sets the download buffer.
        /// </summary>
        public byte[] DownloadBuffer { get; set; }

        /// <summary>
        /// Gets or sets the download byte queue.
        /// </summary>
        public List<byte> DownloadByteQueue { get; set; }

        /// <summary>
        /// Gets or sets the file decryptor.
        /// </summary>
        public FileDecryptor FileDecryptor { get; set; }

        /// <summary>
        /// Gets or sets the optional parameter.
        /// </summary>
        public object OptionalParameter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadHandler"/> class.
        /// </summary>
        public DownloadHandler(
            AesCryptoServiceProvider aes, 
            string fileName,
            string filePath,
            long fileSize, 
            Socket sender,
            object optParam = null)
            :base(aes, fileName, filePath, fileSize)  {
            // Declare variables
            this.DownloadBuffer    = new byte[26400];
            this.DownloadByteQueue = new List<byte>();
            this.DownloadSocket    = sender;
            this.OptionalParameter = optParam;

            // Generate the id for this download
            GenerateID();
        }

        /// <summary>
        /// Generates the identifier for this download.
        /// </summary>
        private void GenerateID()
        {
            // Create placeholder for raw id bytes
            byte[] rawID = new byte[32];

            // Create a random number generator
            using (RandomNumberGenerator RNG = RandomNumberGenerator.Create())
            {
                // Get random bytes
                RNG.GetBytes(rawID);
            }

            // Create placeholder for the hash
            byte[] hash = new byte[16];

            // Create MD5 hasher
            using (MD5 md5 = MD5.Create())
            {
                // Generate the hash
                hash = md5.ComputeHash(rawID);
            }

            // Get string from hash and set the ID
            this.DownloadID = BitConverter.ToString(hash);
        }
    }
}
