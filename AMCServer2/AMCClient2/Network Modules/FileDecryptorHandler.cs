namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    /// Carries information about a download
    /// </summary>
    internal class FileDecryptorHandler
    {
        #region File properties

        /// <summary>
        /// Name of the file being downloaded
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSize { get; private set; }
        /// <summary>
        /// Returns the actual size of the file
        /// </summary>
        public long ActualSize { get; private set; }

        #endregion

        #region Sender

        /// <summary>
        /// Who sends the file?
        /// </summary>
        public ClientViewModel Sender { get; set; }

        #endregion

        #region Private members

        /// <summary>
        /// Writer that will create the file
        /// </summary>
        private FileStream Writer { get; set; }
        /// <summary>
        /// Decryptor
        /// </summary>
        private AesCryptoServiceProvider Decryptor;
        /// <summary>
        /// Crypto
        /// </summary>
        private ICryptoTransform Crypto;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Decryptor">Aes decryptor</param>
        /// <param name="FileName">Name of the file</param>
        /// <param name="Path">Where to create the file?</param>
        /// <param name="FileSize">Size of the file</param>
        public FileDecryptorHandler(AesCryptoServiceProvider Decryptor, string FileName, string Path, long FileSize)
        {
            // Set file properties
            this.Decryptor = Decryptor;
            this.FileName = FileName;
            this.FileSize = FileSize;

            // Create the decryptor transform
            Crypto = Decryptor.CreateDecryptor();

            // Create the writer
            Writer = new FileStream($"{Path}\\{FileName}", FileMode.Create, FileAccess.Write);
        }

        #region Functions

        /// <summary>
        /// Encrypts the block and writes it to the file
        /// </summary>
        /// <param name="Block">Encrypted byte block</param>
        /// <returns>True if file is complete</returns>
        public bool WriteBytes(byte[] Block)
        {
            // Decrypt the block
            byte[] Decrypted = Crypto.TransformFinalBlock(Block, 0, Block.Length);

            // Write the dectypted block to the filestream
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
