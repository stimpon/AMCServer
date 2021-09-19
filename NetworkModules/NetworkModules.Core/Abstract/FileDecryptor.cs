/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Core
{
    #region Namespaces
    using System.IO;
    using System.Security.Cryptography;
    #endregion

    /// <summary>
    /// Handles file decryption
    /// </summary>
    public class FileDecryptor
    {
        #region File properties

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The size of the file
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// The actual size of the file
        /// </summary>
        public long ActualSize { get; private set; }

        #endregion

        #region Private members

        /// <summary>
        /// Writer that will create the file
        /// </summary>
        protected FileStream Writer { get; }

        /// <summary>
        /// The decryptor
        /// </summary>
        protected ICryptoTransform Crypto { get; }

        /// <summary>
        /// Decryptor
        /// </summary>
        protected AesCryptoServiceProvider Decryptor { get; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Decryptor">Aes decryptor</param>
        /// <param name="FileName">Name of the file</param>
        /// <param name="Path">Where to create the file?</param>
        /// <param name="FileSize">Size of the file</param>
        public FileDecryptor(AesCryptoServiceProvider Decryptor, string FileName, string Path, long FileSize)
        {
            // Set file properties
            this.Decryptor = Decryptor;
            this.FileName = FileName;
            this.FileSize = FileSize;

            // Create the decryptor
            Crypto = Decryptor.CreateDecryptor();

            // Create a new FileStream
            Writer = new FileStream($"{Path}\\{FileName}", FileMode.Create,
                                                           FileAccess.Write);
        }

        #region Functions

        /// <summary>
        /// Decrypts the bytes and writes them to the current file
        /// </summary>
        /// <param name="Block">Encrypted byte block</param>
        /// <returns>True if file is complete</returns>
        public bool WriteBytes(byte[] Block)
        {
            // Decrypt the bytes
            byte[] Decrypted = Crypto.TransformFinalBlock(Block, 0, Block.Length);

            // Write the dectypted bytes to the filestream
            Writer.Write(Decrypted);

            // Update the actual size
            ActualSize = Writer.Length;

            // Check if file is complete
            if (ActualSize == FileSize)
            {
                // Close the filestream
                Writer.Close();
                // File is complete
                return true;
            }
            // File is not complete
            else return false;
        }

        #endregion
    }
}
